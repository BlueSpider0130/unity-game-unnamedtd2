using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnscriptedLogic.Builders;
using UnscriptedLogic.Currency;
using UnscriptedLogic.MathUtils;

public class OnProjectileHitEventArgs : EventArgs
{
    public UnitBase unit;
    public ProjectileBase projectile;
    public Action<UnitBase, float> ApplyDamageMethod;
}

[DefaultExecutionOrder(0)]
public class TowerBase : MonoBehaviour, IBuildable, IInspectable
{
    public enum TargetSortMode
    {
        First,
        Last,
        Closest,
        Furthest,
        Strongest,
        Weakest
    }

    [Serializable]
    public class RotationHeads
    {
        public Transform rotationHead;
        public bool levelled;
    }

    [Serializable]
    public class AudioFields
    {
        public AudioClip clip;
        public float volume;
    }

    [Header("Base Tower Settings")]
    [SerializeField] private string id = "tower[TYPE]";
    [SerializeField] private float damage = 1f;
    [SerializeField] private float range = 1f;
    [SerializeField] private float reloadTime = 1f;

    private CurrencyHandler damageHandler;
    private CurrencyHandler rangeHandler;
    private CurrencyHandler reloadTimeHandler;

    [Header("Base Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private int pierce = 1;
    [SerializeField] private int penetratePercent = 0;

    private CurrencyHandler pierceHandler;
    private CurrencyHandler penetrateHandler;

    [Header("Base Components")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected GameObject[] projectilePrefabs;
    [SerializeField] protected RotationHeads[] rotationHeads;
    [SerializeField] protected Transform[] shootAnchors;

    [Space(10)]
    [SerializeField] protected AudioFields[] audioFields;

    [Header("Base Settings")]
    [SerializeField] protected bool drawGizmos;
    [SerializeField] protected float losBaseOffset = 0.5f;
    [SerializeField] protected LayerMask losObstructionLayer;
    [SerializeField] protected LayerMask unitLayer;
    [SerializeField] protected TargetSortMode targetSortMode = TargetSortMode.First;
    [SerializeField] protected SkinnedMeshRenderer towerMeshRenderer;

    protected FXManager soundManager;
    protected TowerDefenseManager tdManager;
    protected float _reloadTime;
    protected Transform currentTarget;
    protected List<Transform> targetsInRange = new List<Transform>();

    public string ID => id;
    public float Damage { get => damageHandler.Current; set { damageHandler.Modify(ModifyType.Set, value); } }
    public float Range { get => rangeHandler.Current; set { rangeHandler.Modify(ModifyType.Set, value); } }
    public float ReloadTime { get => reloadTimeHandler.Current; set { reloadTimeHandler.Modify(ModifyType.Set, value); } }
    public float CurrentReloadtime { get => _reloadTime; set { _reloadTime = value; } }

    public float ProjectileSpeed { get => projectileSpeed; set { projectileSpeed = value; } }
    public float ProjectileLifetime { get => projectileLifetime; set { projectileLifetime = value; } }
    public int ProjectilePierce { get => (int)pierceHandler.Current; set { pierceHandler.Modify(ModifyType.Set, value); } }
    public int ProjectilePenetrate { get => (int)penetrateHandler.Current; set { penetrateHandler.Modify(ModifyType.Set, value); } }

    public CurrencyHandler DamageHandler => damageHandler;
    public CurrencyHandler RangeHandler => rangeHandler;
    public CurrencyHandler ReloadTimeHandler => reloadTimeHandler;
    public CurrencyHandler PierceHandler => pierceHandler;
    public CurrencyHandler PenetrateHandler => penetrateHandler;

    public GameObject[] ProjectilePrefabs => projectilePrefabs;
    public Transform[] ShootAnchors => shootAnchors;

    public Action<UnitBase, float> DamageToBeApplied;
    public Action<UnitBase, float> ApplyDamage;
    public Action<Transform> OnTowerTargetFound;
    public Action<Transform> WhileTowerTargetFound;
    public Action OnTowerTargetLost;
    public Action<GameObject, ProjectileBase> OnTowerProjectileCreated;
    public Action OnTowerProjectileFired;
    public Action<ProjectileBase> OnTowerProjectileDestroyed;
    public event EventHandler<OnProjectileHitEventArgs> OnProjectileHitEvent;

    public static event EventHandler OnAnyTowerSpawned;
    public static event EventHandler OnAnyTowerDespawned;

    public TargetSortMode TargetMode => targetSortMode;
    public SkinnedMeshRenderer TowerMeshRenderer => towerMeshRenderer;

    public virtual void LocalPassBuildConditions<T>(T builder, out List<LocalBuildCondition> localBuildConditions)
    {
        BuildManager buildManager = builder as BuildManager;
        localBuildConditions = new List<LocalBuildCondition>()
        {
            new LocalBuildCondition("Test", (pos, rot) => true, "Test Failed", "Test Succeeded"),
            //new LocalBuildCondition("Cost", (pos, rot) => tdManager.AllTowerList.GetSOFromTower(this).TowerCost <= tdManager.CurrentCash, "Insufficient Cash", "Sufficient Cash")
        };
    }

    protected virtual void Awake()
    {
        //rangeCollider.radius = range;
        damageHandler = new CurrencyHandler(damage);
        rangeHandler = new CurrencyHandler(range);
        reloadTimeHandler = new CurrencyHandler(reloadTime, min: 0.05f);

        pierceHandler = new CurrencyHandler(pierce);
        penetrateHandler = new CurrencyHandler(penetratePercent);

        _reloadTime = 0.5f;
    }

    private void OnEnable()
    {
        tdManager = TowerDefenseManager.instance;
        soundManager = FXManager.instance;

        OnAnyTowerSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void OnDisable()
    {
        OnAnyTowerDespawned?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void Update()
    {
        if (currentTarget != null)
        {
            WhileTargetFound();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (currentTarget != null)
        {
            if (LostTarget())
            {
                currentTarget = null;
                OnTargetLost();
                return;
            }
        }

        if (targetsInRange.Count > 0)
        {
            targetsInRange.Clear();
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, rangeHandler.Current, unitLayer);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (HasLOSToTarget(colliders[i].transform, out RaycastHit losObstruct))
            {
                targetsInRange.Add(colliders[i].transform);
            }
        }

        if (targetsInRange.Count > 0)
        {
            SortTargets();
        }
    }

    #region Tower Logic

    protected void SortTargets()
    {
        Transform target = null;

        switch (targetSortMode)
        {
            case TargetSortMode.First:
                //Node closest to the end point
                target = targetsInRange.OrderByDescending(t => t.GetComponent<UnitBase>().CurrentWaypoint).FirstOrDefault();
                break;
            case TargetSortMode.Last:
                //Node furthest from the end point
                target = targetsInRange.OrderBy(t => t.GetComponent<UnitBase>().CurrentWaypoint).FirstOrDefault();
                break;
            case TargetSortMode.Closest:
                //Closest distance to tower
                target = targetsInRange.OrderBy(t => Vector3.Distance(t.position, transform.position)).FirstOrDefault();
                break;
            case TargetSortMode.Furthest:
                //Furthest distance to tower
                target = targetsInRange.OrderByDescending(t => Vector3.Distance(t.position, transform.position)).FirstOrDefault();
                break;
            case TargetSortMode.Strongest:
                //Highest health point in range
                target = targetsInRange.OrderByDescending(t => t.GetComponent<UnitBase>().CurrentHealth).FirstOrDefault();
                break;
            case TargetSortMode.Weakest:
                //Lowest health point in range
                target = targetsInRange.OrderBy(t => t.GetComponent<UnitBase>().CurrentHealth).FirstOrDefault();
                break;
            default:
                break;
        }

        if (currentTarget == null || target != currentTarget)
        {
            currentTarget = target;
            OnTargetFound();
        }
    }

    protected bool HasLOSToTarget(Transform target, out RaycastHit hit)
    {
        Vector3 origin = transform.position + Vector3.up * losBaseOffset;
        return !Physics.Raycast(origin, (target.position - origin).normalized, out hit, Vector3.Distance(target.position, origin), losObstructionLayer);
    }

    protected bool LostTarget()
    {
        if (!currentTarget.gameObject.activeInHierarchy)
        {
            return true;
        }

        if (!HasLOSToTarget(currentTarget, out RaycastHit losObstruction))
        {
            return true;
        }

        if (Vector3.Distance(currentTarget.position, transform.position) >= rangeHandler.Current + 1f)
        {
            return true;
        }

        return false;
    }

    public void RotateTargettingForward()
    {
        targetSortMode++;
    }

    public void RotateTargettingBackward()
    {
        targetSortMode--;
    }

    protected virtual void CommonTowerLogic()
    {
        if (_reloadTime <= 0f)
        {
            if (currentTarget != null)
            {
                for (int i = 0; i < rotationHeads.Length; i++)
                {
                    RotateToTarget(rotationHeads[i].rotationHead, levelled: rotationHeads[i].levelled);
                }

                _reloadTime = reloadTimeHandler.Current;
                FireProjectile();
            }
        }
        else
        {
            _reloadTime -= Time.deltaTime;
        }
    }

    protected void RotateToTarget(Transform rotationHead, Transform target = null, bool levelled = false)
    {
        if (target == null)
        {
            target = currentTarget;
        }

        if (currentTarget == null)
        {
            return;
        }

        Vector3 targetPos = target.position;
        if (levelled)
        {
            targetPos.y = rotationHead.position.y;
        }

        rotationHead.LookAt(targetPos);
    }

    #endregion

    #region Projectile Handling

    public GameObject CreateBullet(out ProjectileBase projectileBase, GameObject prefab, Transform anchor, ProjectileBehaviour projectileBehaviour = null)
    {
        return CreateBullet(out projectileBase, prefab, anchor.position, anchor.rotation, new ProjectileSettings(ProjectileSpeed, ProjectileLifetime, ProjectilePierce, unitLayer), projectileBehaviour);
    }

    public GameObject CreateBullet(out ProjectileBase projectileBase, GameObject prefab, Vector3 position, Quaternion rotation, ProjectileBehaviour projectileBehaviour = null)
    {
        return CreateBullet(out projectileBase, prefab, position, rotation, new ProjectileSettings(ProjectileSpeed, ProjectileLifetime, ProjectilePierce, unitLayer), projectileBehaviour);
    }

    public GameObject CreateBullet(out ProjectileBase projectileBase, GameObject prefab, Vector3 position, Quaternion rotation, ProjectileSettings projectileSettings, ProjectileBehaviour projectileBehaviour = null)
    {
        //GameObject bullet = PoolManager.poolManagerInstance.PullFromPool(prefab, position, rotation, false);
        GameObject bullet = Instantiate(prefab, position, rotation);
        projectileBase = bullet.GetComponent<ProjectileBase>();

        projectileBase.InitializeAndSetActive(projectileSettings, projectileBehaviour);
        return bullet;
    }

    public virtual void SubscribeProjectileEvents(ProjectileBase projectileBase)
    {
        projectileBase.OnEnemyHit += OnProjectileHit;
        projectileBase.OnProjectileDestroyed += OnProjectileDestroyed;
    }

    public virtual void UnsubscribeProjectileEvents(ProjectileBase projectileBase)
    {
        projectileBase.OnEnemyHit -= OnProjectileHit;
        projectileBase.OnProjectileDestroyed -= OnProjectileDestroyed;
    }

    #endregion

    #region Override Functions

    protected virtual void OnTargetFound()
    {
        OnTowerTargetFound?.Invoke(currentTarget);
    }

    protected virtual void WhileTargetFound()
    {
        CommonTowerLogic();

        WhileTowerTargetFound?.Invoke(currentTarget);
    }

    protected virtual void OnTargetLost()
    {
        OnTowerTargetLost?.Invoke();
    }

    protected virtual void FireProjectile()
    {
        GameObject bullet = CreateBullet(out ProjectileBase projectileBase, projectilePrefabs[0], shootAnchors[0]);
        SubscribeProjectileEvents(projectileBase);

        OnTowerProjectileCreated?.Invoke(bullet, projectileBase);
    }

    protected virtual void OnProjectileFired()
    {
        OnTowerProjectileFired?.Invoke();
    }

    protected virtual void OnProjectileHit(UnitBase unit, ProjectileBase projectileBase)
    {
        float incomingDamage = damageHandler.Current;

        if (ApplyDamage == null)
        {
            ApplyDamage = DamageUnit;
        }

        DamageToBeApplied?.Invoke(unit, incomingDamage);
        OnProjectileHitEvent?.Invoke(this, new OnProjectileHitEventArgs()
        {
            unit = unit,
            projectile = projectileBase,
            ApplyDamageMethod = ApplyDamage,
        });

        ApplyDamage(unit, incomingDamage);
    }

    public void DamageUnit(UnitBase unit, float damage) => unit.TakeDamage(damage);

    protected virtual void OnProjectileDestroyed(ProjectileBase projectile)
    {
        UnsubscribeProjectileEvents(projectile);

        OnTowerProjectileDestroyed?.Invoke(projectile);
    }

    #endregion

    private void OnValidate()
    {
        if (towerMeshRenderer == null)
        {
            SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer != null)
            {
                towerMeshRenderer = meshRenderer;
            }
        }

        if (animator == null)
        {
            Animator animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                this.animator = animator;
            }
        }
    }
}
