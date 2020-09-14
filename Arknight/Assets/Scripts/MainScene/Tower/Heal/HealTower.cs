﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealTower : TowerManager
{
    // 프로퍼티
    public int TileX
    {
        set
        {
            m_TileX = value;
        }
        get
        {
            return m_TileX;
        }
    }
    public int TileY
    {
        set
        {
            m_TileY = value;
        }
        get
        {
            return m_TileY;
        }
    }

    public float CurrentHp
    {
        set
        {
            m_CurrentHp = value;
        }
        get
        {
            return m_CurrentHp;
        }
    }

    public float CurrentMp
    {
        set
        {
            m_CurrentMp = value;
        }
        get
        {
            return m_CurrentMp;
        }
    }

    public float MaxHp
    {
        get
        {
            return m_MaxHp;
        }
    }
    public float MaxMp
    {
        get
        {
            return m_MaxMp;
        }
    }

    public float Damage
    {
        set
        {
            m_Damage = value;
        }
        get
        {
            return m_Damage;
        }
    }

    // 주변의 타워 담긴 리스트
    public List<GameObject> m_AroundTowerList;

    // 주변에 타워 설치되면 주변 타워 리스트에 담아주는 함수 델리게이트
    public DelVoid m_DelAddTower;

    // 주변의 타워가 제거되면 주변의 타워가 담긴 리스트에서도 제거해주는 함수 델리게이트
    public DelDelete m_DelDeleteTower;

    [Header("Unity Stuff")]
    public Image HealthBar2;

    new void Start()
    {
        // 컴포넌트 추가
        base.Start();

        // 델리게이트에 함수 추가
        this.GetComponentInChildren<TowerAnimationEvent>().m_Recovery = new DelAttack(RecoveryTower);
        this.GetComponentInChildren<TowerAnimationEvent>().m_HealTowerSkill = new DelCor(ActiveSkill);

        // 초기화
        Init(100, 10, 2, 5.0f, 3.0f);

        // Add함수 델리게이트 추가
        m_DelAddTower = new DelVoid(AddTower);
        m_DelAddTower?.Invoke();

        // delete함수 델리게이트 추가
        m_DelDeleteTower = new DelDelete(RemoveTower);

        // Animation의 Dead값 false로 설정
        m_Anim.SetBool("Dead", false);
        m_Anim.SetBool("Attack", false);
    }
    // Update is called once per frame
    void Update()
    {
        // FSM 업데이트
        StateProcess();

        // 체력이 0이되면 DEATH로 State 변경
        if (CurrentHp <= 0.0f)
        {
            ChangeState(STATE.DEATH);
        }

        // 타겟이 missing되면 Idle로 변경
        if (m_Target == null || m_Target.activeSelf == false)
        {
            m_Target = null;
            ChangeState(STATE.IDLE);
        }

        // AroundList에도 missing된 오브젝트 제거
        for (int i = 0; i < m_AroundTowerList.Count; ++i)
        {
            if (m_AroundTowerList[i] == null || m_AroundTowerList[i] == false)
            {
                m_AroundTowerList.Remove(m_AroundTowerList[i]);
                --i;
            }
        }
    }
    protected override void ChangeState(STATE s)
    {
        if (m_State == s) return;
        m_State = s;

        switch (m_State)
        {
            case STATE.IDLE:
                m_Target = null;
                break;
            case STATE.BATTLE:
                break;
            case STATE.DEATH:
                m_Anim.SetBool("Dead", true);
                break;
        }
    }

    // 프레임 마다 업데이트 할 함수
    protected override void StateProcess()
    {
        switch (m_State)
        {
            case STATE.IDLE:
                Idle();
                break;
            case STATE.BATTLE:
                Attack();
                break;
            case STATE.DEATH:
                break;
        }
    }

    //대기 상태일때 돌릴 업데이트
    void Idle()
    {
        for (int i = 0; i < m_AroundTowerList.Count; ++i)
        {
            if (m_AroundTowerList[i] == null || m_AroundTowerList[i] == false)
            {
                m_AroundTowerList.Remove(m_AroundTowerList[i]);

                if (m_AroundTowerList.Count == 0)
                    break;
                else
                    --i;
            }
        }

            // 주변에 타워가 없으면 리턴
            if (m_AroundTowerList.Count == 0) 
            return;

        // 주변의 타워를 전부 조사함
        for (int i = 0; i < m_AroundTowerList.Count; ++i)
        {
            // 주변 타워의 체력 담을 변수
            float maxHp = 0;
            float currentHp = 0;

            // 해당 원소가 BasicTower라면
            if (m_AroundTowerList[i].layer == LayerMask.NameToLayer("BasicTower"))
            {
                // 사망하면 넘어감
                if (m_AroundTowerList[i].GetComponent<BasicTower>().m_State == STATE.DEATH)
                {
                    continue;
                }

                // 체력 담아줌
                maxHp = m_AroundTowerList[i].GetComponent<BasicTower>().MaxHp;
                currentHp = m_AroundTowerList[i].GetComponent<BasicTower>().CurrentHp;
            }

            // 해당 원소가 HealTower라면
            else if (m_AroundTowerList[i].layer == LayerMask.NameToLayer("HealTower"))
            {
                // 사망하면 넘어감
                if (m_AroundTowerList[i].GetComponent<HealTower>().m_State == STATE.DEATH)
                {
                    continue;
                }

                // 체력 담아줌
                maxHp = m_AroundTowerList[i].GetComponent<HealTower>().MaxHp;
                currentHp = m_AroundTowerList[i].GetComponent<HealTower>().CurrentHp;
            }
            else if (m_AroundTowerList[i] == null || m_AroundTowerList[i] == false)
            {
                m_AroundTowerList.Remove(m_AroundTowerList[i]);

                if (m_AroundTowerList.Count == 0)
                    break;
                else
                {
                    --i;
                    continue;
                }
            }


                // 현재 HP가 최대 HP보다 작다면
            if (currentHp < maxHp)
            {
                // State를 Battle로 변경 후 함수 빠져나옴
                m_Target = m_AroundTowerList[i];
                ChangeState(STATE.BATTLE);
                break;
            }
            // 아니라면
            else
            {
                // 다음거 검사
                continue;
            }
        }
    }
    
    // 적 방향으로 회전
    void Rotation(GameObject enemy)
    {
        if (enemy == null) return;

        // 적과의 방향 구함
        Vector3 dir = enemy.transform.position -
            this.transform.position;

        // 해당 방향으로 회전
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
            Quaternion.LookRotation(dir), Time.smoothDeltaTime * 360.0f);
    }

    //체력바
    public void HealHealth()
    {
        HealthBar2.fillAmount = m_CurrentHp / m_MaxHp;
        //Debug.Log(HealthBar2.fillAmount.ToString());

    }

    // 공격하는 함수 (HealTower는 힐을 해줌)
    void RecoveryTower(GameObject obj)
    {
        // 오브젝트가 사라지면 Missing object가 나오는데 missing 걸러주는 조건문
        if (obj == null || obj.activeSelf == false)
        {
            ChangeState(STATE.IDLE);
            return;
        }

        // 타겟이 BasicTower라면
        if (obj.layer == LayerMask.NameToLayer("BasicTower"))
        {
            // 타겟이 죽었다면
            if (obj.GetComponent<BasicTower>().m_State == STATE.DEATH)
            {
                // IDLE상태로 변경 후 함수 빠져나옴
                ChangeState(STATE.IDLE);
                return;
            }

            // hp를 회복 시켜줌
            obj.GetComponent<BasicTower>().CurrentHp += m_Damage;
            //basic tower체력바 
            obj.GetComponent<BasicTower>().BasicHealth();

            // 회복시켰는데 MaxHp를 넘어섰다면
            if (obj.GetComponent<BasicTower>().CurrentHp >= obj.GetComponent<BasicTower>().MaxHp)
            {
                // 현재 체력을 MaxHp로 변경 후 IDLE로 변경
                obj.GetComponent<BasicTower>().CurrentHp = obj.GetComponent<BasicTower>().MaxHp;
                //basic tower체력바
                obj.GetComponent<BasicTower>().BasicHealth();

                ChangeState(STATE.IDLE);
            }
        }

        // HealTower 라면 (함수 안 내용은 위와 동일)
        else if (obj.layer == LayerMask.NameToLayer("HealTower"))
        {
            if (obj.GetComponent<HealTower>().m_State == STATE.DEATH)
            {
                ChangeState(STATE.IDLE);
                return;
            }

            obj.GetComponent<HealTower>().CurrentHp += m_Damage;
            //힐타워 체력바
            HealHealth();

            if (obj.GetComponent<HealTower>().CurrentHp >= obj.GetComponent<HealTower>().MaxHp)
            {
                obj.GetComponent<HealTower>().CurrentHp = obj.GetComponent<HealTower>().MaxHp;
                //힐타워 체력바
                HealHealth();

                ChangeState(STATE.IDLE);
            }
        }
        
        if (obj == null)
            ChangeState(STATE.IDLE);
    }

    protected override void Attack()
    {
        if (m_Target != null || m_Target.activeSelf == true ||
            m_Target.GetComponent<BasicTower>().m_State != STATE.DEATH || m_Target.GetComponent<HealTower>().m_State != STATE.DEATH)
        {
            Rotation(m_Target);
        }
           

        // 타겟이 없으면 리턴
        if (m_Target == null) return;

        // 딜레이가 0이하가 된다면
        if (m_AttackDelay <= Mathf.Epsilon)
        {
            // 제일 가까운 적이 살아있다면
            if (m_Target != null || m_Target == null || m_Target.activeSelf == false)
            {
                // Attack 트리거 발동
                m_Anim.SetTrigger("Attack");

                // 다시 딜레이 설정
                m_AttackDelay = 3.0f;
            }
            // 적이 죽었다면
            else
            {
                // IDLE상태로 바꿈
                ChangeState(STATE.IDLE);
            }
        }

        // 딜레이 감소
        m_AttackDelay -= Time.deltaTime;
        
    }

    // 주변에 타워가 있으면 리스트에 추가해주는 함수
    void AddTower()
    {
        // Y축 좌표 검사
        for (int i = TileY - 1; i < TileY + 2; ++i)
        {
            // 타일 범위 벗어나면 넘어감 (세로)
            if (i < 0 || i > 8)
                continue;

            // X축 좌표 검사
            for (int j = TileX - 1; j < TileX + 2; ++j)
            {
                // 타일 범위 벗어나면 넘어감 (가로)
                if (j < 0 || j > 9)
                    continue;

                // 해당 좌표의 노드상태가 TOWER라면 (타워가 설치됬다면)
                if (m_NodeManager.m_TileState[i, j] == NodeManager.TILEINFO.TOWER)
                {
                    // 해당 좌표의 타워를 저장
                    GameObject temp = m_BuildManager.GetTowerCoordinates(j, i);

                    // 리스트가 비어있으면
                    if (m_AroundTowerList.Count == 0)
                    {
                        // 바로 담아준다
                        m_AroundTowerList.Add(temp);
                    }
                    // 비어있지 않으면
                    else
                    {
                        // 해당 리스트 조사해서
                        for (int k = 0; k < m_AroundTowerList.Count; ++k)
                        {
                            // 해당 리스트의 원소와 같으면
                            if (m_AroundTowerList[k] == temp)
                            {
                                // 넘어감
                                continue;
                            }
                            // 해당 리스트의 원소와 다르면
                            else
                            {
                                // 리스트에 담아줌
                                m_AroundTowerList.Add(temp);
                                break;
                            }
                        }
                    }
                }
            }
        }

        // 리스트를 돌아서 중복검사 해줌
        for (int i = 0; i < m_AroundTowerList.Count; ++i)
        {
            for (int j = i + 1; j < m_AroundTowerList.Count; ++j)
            {
                // 검사하려는 타워가 또 있다면
                if (m_AroundTowerList[i] == m_AroundTowerList[j])
                {
                    // 뒤쪽의 타워를 제거해줌
                    m_AroundTowerList.Remove(m_AroundTowerList[j]);

                    // 사이즈가 1줄었으므로 해당 원소를 다시 조사하기 위해 -1해줌
                    --j;
                }
            }
        }
    }

    // 주변에 있는 타워 제거되면 리스트에서도 지워주는 함수
    public void RemoveTower(GameObject tower)
    {
        // 리스트가 비어있으면 리턴
        if (m_AroundTowerList.Count == 0) return;

        // 리스트 돌아서
        for (int i = 0; i < m_AroundTowerList.Count; ++i)
        {
            // 해당 리스트에 해당타워가 있으면
            if (m_AroundTowerList[i].transform == tower.transform)
            {
                // 리스트에서 제거 후 빠져나옴 (더이상 검사할 필요 x)
                m_AroundTowerList.Remove(m_AroundTowerList[i]);
                break;
            }
        }
    }

    public override IEnumerator ActiveSkill(float timer)
    {
        while(true)
        {
            if (m_Target == null) break;
            float originDmg = 0.0f;

            if (m_Target.layer == LayerMask.NameToLayer("BasicTower"))
            {
                originDmg = m_Target.GetComponent<BasicTower>().Damage;
                m_Target.GetComponent<BasicTower>().Damage *= 2.0f;
                yield return new WaitForSeconds(timer);
                m_Target.GetComponent<BasicTower>().Damage = originDmg;
                break;
            }
            else if (m_Target.layer == LayerMask.NameToLayer("HealTower"))
            {
                originDmg = m_Target.GetComponent<HealTower>().Damage;
                m_Target.GetComponent<HealTower>().Damage *= 2.0f;
                yield return new WaitForSeconds(timer);
                m_Target.GetComponent<HealTower>().Damage = originDmg;
                break;
            }
        }
    }
}