﻿using Google.Protobuf.Protocol;
using Server.Data;
using Server.DB;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;

namespace Server.Game
{
	public class Monster : GameObject
	{
		protected Vector3 SpawnPos;
		public bool isMoving = false;
		public int TemplateId { get; private set; }
		public GameObject Target { get; protected set; }
		public bool isCanAttack = false;
		public bool isMotion = false;
		public Vector3 forwardMonster = new Vector3 (0, 0, 0);
		public Monster()
		{
			ObjectType = GameObjectType.Monster;
		}

		public virtual void Init(int templateId)
		{
			TemplateId = templateId;

			MonsterData monsterData = null;
			DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);
			Info.Name = monsterData.name;
			Stat.MergeFrom(monsterData.stat);
			Stat.Hp = monsterData.stat.MaxHp;
			State = CreatureState.Idle;
		}

		// FSM (Finite State Machine)
		IJob _job;
		public override void Update()
		{
            //Console.WriteLine(State);

            switch (State)
			{
				case CreatureState.Idle:
					UpdateIdle();
					break;
				case CreatureState.Moving:
					UpdateMoving();
					break;
				case CreatureState.Skill:
					UpdateSkill();
					break;
				case CreatureState.Dead:
					UpdateDead();
					break;
				case CreatureState.Wait:
					break;
			}
            // 5프레임 (0.2초마다 한번씩 Update)
            if (Room != null)
				_job = Room.PushAfter(200, Update);
		}
		protected Positions nextPos = new Positions();
		public long _nextSearchTick = 0;
		protected virtual void UpdateIdle()
		{
			if (Room.PlayerCount < 1) return;
			if (State != CreatureState.Idle) return;
            if (_nextSearchTick == 0)
                _nextSearchTick = Environment.TickCount64 + 7000;
            if (_nextSearchTick > Environment.TickCount64)
				return;
			_nextSearchTick = Environment.TickCount64 + 7000;
			Vector3 curPos = Utils.PositionsToVector3(Pos);
			/*if(Vector3.Distance(curPos, SpawnPos) > 20)
			{
				nextPos.PosX = SpawnPos.x;
				nextPos.PosY = SpawnPos.y + 1;
				nextPos.PosZ = SpawnPos.z;
            }
			else*/
			{
                Random random = new Random();
                nextPos.PosX = Pos.PosX + random.Next(-5, 5);
                nextPos.PosY = PosInfo.Pos.PosY + 1;
                nextPos.PosZ = Pos.PosZ + random.Next(-5, 5);
            }
            
            State = CreatureState.Moving;
		}

		protected virtual void UpdateMoving()
		{
            if (isMoving == true) return;
            if (Room.PlayerCount < 1) return;
            
			BroadcastMove();
		}

		protected void BroadcastMove()
		{
			// 다른 플레이어한테도 알려준다
			isMoving = true;

            S_Move movePacket = new S_Move();
			movePacket.ObjectId = Id;
            PositionInfo nextPosinfo = new PositionInfo();
			nextPosinfo.MergeFrom(PosInfo);
			nextPosinfo.Pos = nextPos;
			movePacket.DestPosInfo = nextPosinfo;
			if(Target == null)
				movePacket.TargetId = -1;
			else movePacket.TargetId = Target.Id;
            Room.Broadcast(movePacket);
        }

        protected long _coolTick = 0;
        protected long _checkDisTick = 0;
        protected long _MoveTick = 0;
        protected virtual void UpdateSkill()
		{
           
            if (_coolTick == 0)
			{
                if (_checkDisTick > Environment.TickCount64)
                    return;
                _checkDisTick = Environment.TickCount64 + 1000;
                // 유효한 타겟인지
                if (Target == null || Target.Room != Room)
				{
                    Target = null;
					State = CreatureState.Idle;
					return;
				}

				Skill skillData = null;
				DataManager.SkillDict.TryGetValue(4, out skillData);


				// 스킬 쿨타임 적용
				int coolTick = (int)(1000 * skillData.cooldown);
				_coolTick = Environment.TickCount64 + coolTick;
			}

			if (_coolTick > Environment.TickCount64)
				return;

			_coolTick = 0;
		}

		protected virtual void UpdateDead()
		{

		}

		public override void OnDead(GameObject attacker)
		{
			if (_job != null)
			{
				_job.Cancel = true;
				_job = null;
			}

			base.OnDead(attacker);

			GameObject owner = attacker.GetOwner();
			if (owner.ObjectType == GameObjectType.Player)
			{
				RewardData rewardData = GetRandomReward();
				if (rewardData != null)
				{
					Player player = (Player)owner;
					DbTransaction.RewardPlayer(player, rewardData, Room);
				}
			}
		}

		RewardData GetRandomReward()
		{
			MonsterData monsterData = null;
			DataManager.MonsterDict.TryGetValue(TemplateId, out monsterData);

			int rand = new Random().Next(0, 101);

			int sum = 0;
			/*foreach (RewardData rewardData in monsterData.rewards)
			{
				sum += rewardData.probability;

				if (rand <= sum)
				{
					return rewardData;
				}
			}
*/
			return null;
		}
        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
			if(Stat.Hp > 0)
			{
				Target = attacker;
                // 추격으로 변환
                S_StopMove resStopMovePacket = new S_StopMove();
                resStopMovePacket.ObjectId = Info.ObjectId;
                resStopMovePacket.PosOk = true;
                resStopMovePacket.Rotate = PosInfo.Rotate;
                resStopMovePacket.Pos = Pos;
                Room.Broadcast(resStopMovePacket);
				_MoveTick = 0;
                State = CreatureState.Wait;
                Console.WriteLine("맞음");
                isCanAttack = false;
                isMotion = false;
                Room.PushAfter(1200, ChangeStateAfterTime, CreatureState.Skill);
            }
        }
		public void ChangeStateAfterTime(CreatureState state)
		{
			State = state;
		}
    }
}
