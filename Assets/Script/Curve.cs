using UnityEngine;
using System.Collections;

public class Curve : MonoBehaviour 
{
    //自身的线渲染器组件
    private LineRenderer selfLineRenderer;

    //数组，存储所有点的坐标
    private Vector3[] vertexArray;

    //曲线每帧的偏移
    private int deltaOffset = 6;

    //曲线的总体偏移
    private int totalOffset;

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
        //自身的线渲染器组件
        selfLineRenderer = this.GetComponent<LineRenderer>();

        //顶点数组初始化
        vertexArray = new Vector3[1440];

        //指定线渲染的顶点数
        selfLineRenderer.SetVertexCount(vertexArray.Length);

        //设置线渲染的线条宽度
        selfLineRenderer.SetWidth(0.05F, 0.05F);

        //循环
        for (int i = 0; i < vertexArray.Length; i++)
        {
            //计算每个顶点的坐标
            vertexArray[i] = new Vector3(0.01F * (i - 720), 3 * Mathf.Sin((i - 720) * Mathf.Deg2Rad), 0);
        }

        //将顶点数组设置到线渲染上
        selfLineRenderer.SetPositions(vertexArray);

        //初始化，总偏移为0
        totalOffset = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //总偏移递增
        totalOffset += deltaOffset;

        //循环
        for (int i = 0; i < vertexArray.Length; i++)
        {

        //设置线渲染的点坐标
            selfLineRenderer.SetPosition(i, new Vector3(vertexArray[i].x, vertexArray[(i + totalOffset) % vertexArray.Length].y, 0));
        }
    }

    //当该脚本组件不可用时
    void OnDisable()
    {
    }
}
