using UnityEngine;
using System.Collections;

public class BornPoint : MonoBehaviour 
{
    //该出生点生成的怪物
    public GameObject targetEnemy;

    //生成怪物的总数量
    public int enemyTotalNum = 10;

    //生成怪物的计数器
    private int enemyCounter;

    //生成怪物的时间间隔
    public float intervalTime = 3;

    //玩家
    private GameObject targetPlayer;

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
        //玩家
        targetPlayer = GameObject.FindGameObjectWithTag("Player");

        //初始时，怪物计数为0
        enemyCounter = 0;

        //重复生成怪物
        InvokeRepeating("CreatEnemy", 0.5F, intervalTime);
    }

    // Update is called once per frame
    void Update()
    {
    }

    //当该脚本组件不可用时
    void OnDisable()
    {
    }

    //方法，生成怪物
    private void CreatEnemy()
    {
        //如果玩家存活
        if (targetPlayer.GetComponent<Player>().currentHp > 0)
        {
            //生成一只怪物
            Instantiate(targetEnemy, this.transform.position, Quaternion.identity);

            //计数
            enemyCounter++;

            //如果计数到达最大值
            if (enemyCounter == enemyTotalNum)
            {
                //停止刷新
                CancelInvoke();
            }
        }

        //玩家死亡
        else
        {
            //停止刷新
            CancelInvoke();
        }
    }
}
