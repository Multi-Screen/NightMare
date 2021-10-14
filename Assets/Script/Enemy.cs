using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : MonoBehaviour 
{
    //总血量
    public int totalHp = 200;

    //当前血量
    private int currentHp;

    //血条的高度
    public float hpBarHeight = 2;

    //血条
    private GameObject selfHpBar;

    //血条的Image组件
    private Image hpBar;

    //攻击的攻击力
    public int attackDamage = 10;

    //攻击的攻击间隔
    public float attackInterval = 1;

    //攻击的距离
    public float attackDistance = 3;

    //寻路的目标物体
    private GameObject navigationTarget;

    //自身的寻路组件
    private UnityEngine.AI.NavMeshAgent selfNavMeshAgent;

    //自身的动画组件
    private Animator selfAnim;

    //攻击的冷却状态
    private bool attackColdState;

    //攻击的冷却计时
    private float attackColdTimer;

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

        //寻路的目标物体
        navigationTarget = GameObject.FindGameObjectWithTag("Player");

        //自身的寻路组件
        selfNavMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();

        //自身的动画组件
        selfAnim = this.GetComponent<Animator>();

        //初始时，攻击未冷却，冷却计时为0
        attackColdState = false;
        attackColdTimer = 0;

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

        //怪物存活
        if (currentHp > 0)
        {
            //实时更新寻路的目标点
            selfNavMeshAgent.SetDestination(navigationTarget.transform.position);

            //怪物攻击
            EnemyAttack();
        }

        //如果攻击冷却
        if (attackColdState)
        {
            //冷却的计时
            attackColdTimer += Time.deltaTime;

            //如果计时到达指定时间
            if (attackColdTimer >= attackInterval)
            {
                //退出冷却
                attackColdState = false;

                //计时器清0
                attackColdTimer = 0;
            }
        }
    }

    //当该脚本组件不可用时
    void OnDisable()
    {
    }

    //方法，怪物攻击
    private void EnemyAttack()
    {
        //如果怪物和玩家距离小于等于攻击距离、怪物攻击未冷却
        if (Vector3.Distance(this.transform.position, navigationTarget.transform.position) <= attackDistance &&
            attackColdState == false)
        {
            //玩家受到伤害
            navigationTarget.GetComponent<Player>().TakeDamage(attackDamage);

            //如果本次伤害造成玩家死亡
            if (navigationTarget.GetComponent<Player>().currentHp <= 0)
            {
                //停止寻路
                selfNavMeshAgent.Stop();

                //怪物的动画从移动转为等待
                selfAnim.SetTrigger("EnemyIdle");
            }

            //攻击冷却
            attackColdState = true;
        }
    }

    //方法，受到伤害
    public void TakeDamage(int damageValue)
    {
        //怪物存活
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
                selfAnim.SetTrigger("EnemyDeath");

                //停止寻路
                selfNavMeshAgent.enabled = false;
            }

            //更新血条的值
            hpBar.fillAmount = currentHp / (float)totalHp;
        }
    }

    //方法，怪物死亡时销毁
    private void DestroySelf()
    {
        Destroy(this.gameObject);

        Destroy(selfHpBar);
    }
}
