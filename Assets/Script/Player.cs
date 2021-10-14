using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//玩家的攻击模式
public enum AttackMode
{
    Normal,
    Special
}

public class Player : MonoBehaviour 
{
    //总血量
    public int totalHp = 200;

    //当前血量
    public int currentHp;

    //血条的高度
    public float hpBarHeight = 2;

    //血条
    private GameObject selfHpBar;

    //血条的Image组件
    private Image hpBar;

    //攻击模式
    public AttackMode attackMode = AttackMode.Normal;

    //攻击力
    public int normalAttackDamage = 25;
    public int specialAttackDamage = 100;

    //攻击间隔
    public float normalAttackInterval = 1;
    public float specialAttackInterval = 1;

    //玩家移动的速度大小
    public float moveSpeed = 5;

    //枪口
    public Transform gunBarrelEnd;

    //激光线
    public LineRenderer laser;

    //自身的刚体组件
    private Rigidbody selfRigidbody;

    //自身的动画组件
    private Animator selfAnim;

    //自身的碰撞体组件
    private CapsuleCollider selfCapsuleCollider;

    //玩家移动的水平和竖直分量
    private float horizontalFactor;
    private float verticalFactor;

    //玩家移动的方向
    private Vector3 moveDirection;

    //玩家的朝向
    private Vector3 faceDirection;

    //射线碰撞的层标记
    private int rotateHitMask;
    private int attackHitMask;

    //射线的碰撞信息
    private RaycastHit rotateHitInfo;
    private RaycastHit normalAttackHitInfo;
    private RaycastHit[] specialAttackHitInfo;

    //枪口的粒子效果
    private ParticleSystem fireParticle;

    //枪口的灯光效果
    private Light fireLight;

    //射击点的粒子效果
    private GameObject hitParticle;
    private List<GameObject> hitParticleList = new List<GameObject>();

    //bool值，指示攻击是否处于冷却状态
    private bool normalAttackColdState;
    private bool specialAttackColdState;

    //攻击冷却计时器
    private float normalAttackColdTimer;
    private float specialAttackColdTimer;

	//唤醒
    void Awake()
    {      
    }

    //当该脚本组件可用时
    void OnEnable()
    {
    }

    // Use this for initialization
    void Start()
    {
        //初始时，满血
        currentHp = totalHp;

        //自身的刚体组件
        selfRigidbody = this.GetComponent<Rigidbody>();

        //自身的动画组件
        selfAnim = this.GetComponent<Animator>();

        //自身的碰撞体组件
        selfCapsuleCollider = this.GetComponent<CapsuleCollider>();

        //枪口的粒子效果
        fireParticle = gunBarrelEnd.GetComponent<ParticleSystem>();

        //枪口的灯光效果
        fireLight = gunBarrelEnd.GetComponent<Light>();

        //射线碰撞的层标记
        rotateHitMask += 1 << LayerMask.NameToLayer("Floor");
        attackHitMask += 1 << LayerMask.NameToLayer("Environment");
        attackHitMask += 1 << LayerMask.NameToLayer("Enemy");

        //初始时，攻击未冷却
        normalAttackColdState = false;
        specialAttackColdState = false;

        //初始时，攻击冷却计时为0
        normalAttackColdTimer = 0;
        specialAttackColdTimer = 0;

        //生成血条
        selfHpBar = Instantiate(Resources.Load<GameObject>("Prefab/HpCanvas"), 
                                this.transform.position + hpBarHeight * Vector3.up, 
                                Quaternion.identity) as GameObject;

        hpBar = selfHpBar.transform.Find("HpBar").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        //血条跟随
        selfHpBar.transform.position = this.transform.position + hpBarHeight * Vector3.up;

        //玩家存活
        if (currentHp > 0)
        {
            //判断攻击模式
            if (attackMode == AttackMode.Normal)
            {
                //玩家普通攻击
                PlayerNormalAttack();
            }

            if (attackMode == AttackMode.Special)
            {
                //玩家特殊攻击
                PlayerSpecialAttack();
            }
        }

        //如果普通攻击冷却
        if (normalAttackColdState)
        {
            //冷却计时
            normalAttackColdTimer += Time.deltaTime;

            //如果计时到达时间间隔
            if (normalAttackColdTimer >= normalAttackInterval)
            {
                //退出冷却
                normalAttackColdState = false;

                //计时器清0
                normalAttackColdTimer = 0;
            }
        }

        //如果特殊攻击冷却
        if (specialAttackColdState)
        {
            //冷却计时
            specialAttackColdTimer += Time.deltaTime;

            //如果计时到达时间间隔
            if (specialAttackColdTimer >= specialAttackInterval)
            {
                //退出冷却
                specialAttackColdState = false;

                //计时器清0
                specialAttackColdTimer = 0;
            }
        }
    }

    //物理帧更新
    void FixedUpdate()
    {
        //玩家存活
        if (currentHp > 0)
        {
            //玩家移动的水平和竖直分量
            horizontalFactor = Input.GetAxis("Horizontal");
            verticalFactor = Input.GetAxis("Vertical");

            //如果水平分量不为0，或竖直分量不为0
            if (horizontalFactor != 0 || verticalFactor != 0)
            {
                //玩家移动
                PlayerMove();
            }

            //切换Idle和Move动画
            selfAnim.SetBool("PlayerMove", horizontalFactor != 0 || verticalFactor != 0);

            //玩家旋转
            PlayerRotate();
        }
    }

    //当该脚本组件不可用时
    void OnDisable()
    {
    }

    //方法，玩家移动
    private void PlayerMove()
    {
        //玩家移动的方向
        moveDirection = new Vector3(horizontalFactor, 0, verticalFactor);

        //归一化
        moveDirection.Normalize();

        //移动
        selfRigidbody.MovePosition(this.transform.position + moveSpeed * moveDirection * Time.fixedDeltaTime);
    }

    //方法，玩家旋转
    private void PlayerRotate()
    {
        //产生一条3D射线
        Ray testRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        //发射射线，记录碰撞信息和碰撞结果
        bool hitResult = Physics.Raycast(testRay, out rotateHitInfo, Camera.main.farClipPlane, rotateHitMask);

        //如果该射线和Floor层产生碰撞
        if (hitResult)
        {
            //计算玩家的面向
            faceDirection = rotateHitInfo.point - this.transform.position;

            //修正Y值
            faceDirection.y = 0;

            //如果交点和玩家之间的距离大于阈值
            if (faceDirection.magnitude >= selfCapsuleCollider.radius)
            {
                //计算玩家的旋转
                Quaternion tempQuaternion = Quaternion.LookRotation(faceDirection);

                //玩家转向该旋转
                selfRigidbody.MoveRotation(tempQuaternion);
            }
        }
    }

    //方法，玩家普通攻击
    private void PlayerNormalAttack()
    {
        //如果按下鼠标左键
        if (Input.GetAxis("Fire1") > 0)
        {
            //激光线激活
            laser.gameObject.SetActive(true);

            //枪口的粒子效果开始播放
            fireParticle.Play();

            //枪口的灯光激活
            fireLight.enabled = true;

            //设置激光线渲染器的起点
            laser.SetPosition(0, gunBarrelEnd.position);

            //以枪口位置为起点，枪口正前方为方向，产生一条3D射线
            Ray testRay = new Ray(gunBarrelEnd.position, gunBarrelEnd.forward);

            //发射射线，记录碰撞信息和碰撞结果
            bool hitResult = Physics.Raycast(testRay, out normalAttackHitInfo, 100, attackHitMask);

            //如果射线和指定层的物体发生碰撞
            if (hitResult)
            {
                //设置激光线渲染器的终点
                laser.SetPosition(1, normalAttackHitInfo.point);

                //如果射击点没有粒子效果
                if (hitParticle == null)
                {
                    //在渲染器的终点处生成一个粒子效果
                    hitParticle = Instantiate(Resources.Load<GameObject>("Prefab/HitParticle"),
                                              normalAttackHitInfo.point,
                                              Quaternion.identity) as GameObject;
                }

                //射击点粒子效果已经生成
                else
                {
                    //更新该粒子效果的位置
                    hitParticle.transform.position = normalAttackHitInfo.point;
                }

                //如果射线碰到的物体是敌人
                if (normalAttackHitInfo.transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    //如果普通攻击未冷却
                    if (normalAttackColdState == false)
                    {
                        //敌人受到伤害
                        normalAttackHitInfo.transform.GetComponent<Enemy>().TakeDamage(normalAttackDamage);

                        //普通攻击进入冷却
                        normalAttackColdState = true;
                    }
                }
            }

            //否则
            else
            {
                //设置激光线渲染器的终点
                laser.SetPosition(1, gunBarrelEnd.position + gunBarrelEnd.forward * 100);
            }
        }

        //否则
        else
        {
            //停止普通攻击
            StopNormalAttack();
        }
    }

    //方法，停止普通攻击
    private void StopNormalAttack()
    {
        //激光线禁用
        laser.gameObject.SetActive(false);

        //枪口的灯光禁用
        fireLight.enabled = false;

        //枪口的粒子效果停止播放
        fireParticle.Stop();

        //销毁射击点粒子效果
        Destroy(hitParticle);
    }

    //方法，玩家特殊攻击
    private void PlayerSpecialAttack()
    {
        //如果按下鼠标左键
        if (Input.GetAxis("Fire1") > 0)
        {
            //激光线激活
            laser.gameObject.SetActive(true);

            //枪口的粒子效果开始播放
            fireParticle.Play();

            //枪口的灯光激活
            fireLight.enabled = true;

            //设置激光线渲染器的起点和终点
            laser.SetPosition(0, gunBarrelEnd.position);
            laser.SetPosition(1, gunBarrelEnd.position + gunBarrelEnd.forward * 100);

            //以枪口位置为起点，枪口正前方为方向，产生一条3D射线
            Ray testRay = new Ray(gunBarrelEnd.position, gunBarrelEnd.forward);

            //发射射线，记录碰撞信息和碰撞结果
            specialAttackHitInfo = Physics.RaycastAll(testRay, 100, attackHitMask);

            //bool值，指示是否有敌人受到伤害
            bool specialDamageValidState = false;

            //如果碰撞信息数组的长度大于0
            if (specialAttackHitInfo.Length > 0)
            {
                //遍历碰撞信息数组
                for (int i = 0; i < specialAttackHitInfo.Length; i++)
                {
                    //如果第i个物体为敌人
                    if (specialAttackHitInfo[i].transform.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                    {
                        //特殊攻击处于未冷却状态
                        if (specialAttackColdState == false)
                        {
                            //第i个敌人受到伤害
                            specialAttackHitInfo[i].transform.GetComponent<Enemy>().TakeDamage(specialAttackDamage);

                            //有至少一个敌人受到伤害
                            specialDamageValidState = true;
                        }
                    }
                }

                //如果至少有一个敌人受到伤害
                if (specialDamageValidState)
                {
                    //特殊攻击进入冷却
                    specialAttackColdState = true;
                }

                //如果需要的粒子的数目大于等于粒子集合中的数目
                if (specialAttackHitInfo.Length >= hitParticleList.Count)
                {
                    //对于粒子集合中已有的粒子
                    for (int i = 0; i < hitParticleList.Count; i++)
                    {
                        //更新位置
                        hitParticleList[i].transform.position = specialAttackHitInfo[i].point;
                    }

                    //额外生成剩余的粒子
                    for (int i = hitParticleList.Count; i < specialAttackHitInfo.Length; i++)
                    {
                        //生成新的粒子
                        hitParticle = Instantiate(Resources.Load<GameObject>("Prefab/HitParticle"),
                                                  specialAttackHitInfo[i].point,
                                                  Quaternion.identity) as GameObject;

                        //将新生成的粒子添加到集合中
                        hitParticleList.Add(hitParticle);
                    }
                }

                //如果需要的粒子的数目小于粒子集合中的数目
                else
                {
                    //从粒子集合中取足够的粒子
                    for (int i = 0; i < specialAttackHitInfo.Length; i++)
                    {
                        //更新位置
                        hitParticleList[i].transform.position = specialAttackHitInfo[i].point;
                    }

                    //多余的粒子
                    for (int i = specialAttackHitInfo.Length; i < hitParticleList.Count; i++)
                    {
                        //销毁
                        Destroy(hitParticleList[i]);
                    }

                    //将已经销毁的粒子从集合中移除
                    hitParticleList.RemoveRange(specialAttackHitInfo.Length, hitParticleList.Count - specialAttackHitInfo.Length);
                }
            }
        }

        //否则
        else
        {
            //停止特殊攻击
            StopSpecialAttack();
        }
    }

    //方法，停止特殊攻击
    private void StopSpecialAttack()
    {
        //激光线禁用
        laser.gameObject.SetActive(false);

        //枪口的灯光禁用
        fireLight.enabled = false;

        //枪口的粒子效果停止播放
        fireParticle.Stop();

        //销毁全部粒子效果
        for (int i = 0; i < hitParticleList.Count; i++)
        {
            Destroy(hitParticleList[i]);
        }

        //集合清空
        hitParticleList.Clear();
    }

    //方法，受到伤害
    public void TakeDamage(int damageValue)
    {
        //玩家存活
        if (currentHp > 0)
        {
            //血量减少
            currentHp -= damageValue;

            //如果血量小于等于0
            if (currentHp <= 0)
            {
                //修正血量为0
                currentHp = 0;

                //播放死亡动画
                selfAnim.SetTrigger("PlayerDeath");

                //判断攻击模式
                if (attackMode == AttackMode.Normal)
                {

                    //停止普通攻击
                    StopNormalAttack();
                }

                if (attackMode == AttackMode.Special)
                {

                    //停止特殊攻击
                    StopSpecialAttack();
                }
            }

            //更新血条的值
            hpBar.fillAmount = currentHp / (float)totalHp;
        }
    }
}
