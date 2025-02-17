using UnityEngine;
using UnityEngine.AI;

public class EnemySceneTest : MonoBehaviour
{
    private GameObject albino;
    private NavMeshAgent agent;
    private Camera mainCamera;

    void Start()
    {
        // 查找名为Albino的敌人
        albino = GameObject.Find("Albino");
        if (albino != null)
        {
            // 获取或添加NavMeshAgent组件
            agent = albino.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = albino.AddComponent<NavMeshAgent>();
            }
        }

        // 获取主相机
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 检测鼠标左键点击
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            // 发射射线检测点击位置
            if (Physics.Raycast(ray, out hit))
            {
                // 如果敌人和导航组件存在，设置目标位置
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.SetDestination(hit.point);
                }
            }
        }
    }
}
