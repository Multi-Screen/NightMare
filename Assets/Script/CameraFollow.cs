using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour 
{
    //摄像机跟随的目标物体
    public Transform followTarget;

    //跟随过程中，摄像机与目标之间的相对位置
    private Vector3 relativePosition;

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
        //摄像机与目标之间的相对位置
        relativePosition = this.transform.position - followTarget.position;
    }

    // Update is called once per frame
    void Update()
    {
    }

    //物理帧更新
    void FixedUpdate()
    {
        //实时更新摄像机的位置
        this.transform.position = followTarget.position + relativePosition;
    }

    //当该脚本组件不可用时
    void OnDisable()
    {
    }
}
