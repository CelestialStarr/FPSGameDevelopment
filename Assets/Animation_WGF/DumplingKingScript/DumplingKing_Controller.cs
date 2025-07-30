using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumplingKing_Controller : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 1.5f;
    public GameObject miniDumplingPrefab;

    [Header("动画子物体")]
    public GameObject body;
    public GameObject leftHand, rightHand, leftLeg, rightLeg;

    [Header("技能参数")]
    public float skillInterval = 5f;
    public float rollSpeed = 5f;
    public float rollHitRadius = 1f;
    public float summonYOffset = 1f;
    public float summonZOffset = 2f;

    private float skillTimer = 0f;
    private bool bornPlayed = false;
    private bool isRolling = false;
    private bool hitPlayer = false;

    void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        PlayAllLimbAnim("born");
        Invoke(nameof(StartWalk), 3f); // 出生动画持续 3 秒
    }

    void StartWalk()
    {
        bornPlayed = true;
        PlayAllLimbAnim("walk");
    }

    void Update()
    {
        if (!bornPlayed || isRolling) return;

        skillTimer += Time.deltaTime;
        if (skillTimer >= skillInterval)
        {
            skillTimer = 0f;
            int skill = Random.Range(0, 2);
            if (skill == 0) StartCoroutine(RollToPlayer());
            else StartCoroutine(SummonMiniDumpling());
        }

        if (player != null)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            if (dir.magnitude > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;
            }
        }
    }

    IEnumerator RollToPlayer()
    {
        isRolling = true;
        PlayOnlyBodyAnim("roll");
        SetLimbsActive(false);

        float timer = 0f;
        hitPlayer = false;
        while (!hitPlayer && timer < 5f)
        {
            Vector3 dir = player.position - transform.position;
            dir.y = 0;
            transform.rotation = Quaternion.LookRotation(dir);
            transform.position += dir.normalized * rollSpeed * Time.deltaTime;

            if (Vector3.Distance(transform.position, player.position) <= rollHitRadius)
            {
                hitPlayer = true;
                Debug.Log("撞到玩家！");
            }

            timer += Time.deltaTime;
            yield return null;
        }

        SetLimbsActive(true);
        PlayAllLimbAnim("walk");
        isRolling = false;
    }

    IEnumerator SummonMiniDumpling()
    {
        PlayAllLimbAnim("wave");
        yield return new WaitForSeconds(1.5f);
        Vector3 spawnPos = transform.position + transform.forward * summonZOffset + Vector3.up * summonYOffset;
        Instantiate(miniDumplingPrefab, spawnPos, Quaternion.identity);
        PlayAllLimbAnim("walk");
    }

    void SetLimbsActive(bool active)
    {
        leftHand.SetActive(active);
        rightHand.SetActive(active);
        leftLeg.SetActive(active);
        rightLeg.SetActive(active);
    }

    void PlayAllLimbAnim(string state)
    {
        if (body) body.GetComponent<LimbAnimatorController>()?.Play(state);
        if (leftHand) leftHand.GetComponent<LimbAnimatorController>()?.Play(state);
        if (rightHand) rightHand.GetComponent<LimbAnimatorController>()?.Play(state);
        if (leftLeg) leftLeg.GetComponent<LimbAnimatorController>()?.Play(state);
        if (rightLeg) rightLeg.GetComponent<LimbAnimatorController>()?.Play(state);
    }

    void PlayOnlyBodyAnim(string state)
    {
        if (body) body.GetComponent<LimbAnimatorController>()?.Play(state);
    }
}
