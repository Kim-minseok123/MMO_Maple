﻿using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.DB
{
	public partial class DbTransaction : JobSerializer
	{
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Equipped = item.Equipped,
                Slot = item.Slot,
            };

            Instance.Push(() => 
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Equipped)).IsModified = true;
                    db.Entry(itemDb).Property(nameof(ItemDb.Slot)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패했다면 유저를 Kick
                    }
                }
            });
        }
        public static void RemoveItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    ItemDb itemDb = db.Items.SingleOrDefault(i => i.ItemDbId == item.ItemDbId);
                    if (itemDb != null)
                    {
                        db.Items.Remove(itemDb);
                        bool success = db.SaveChangesEx();
                        if (!success)
                        {
                            // 실패 시 로직 추가 (로그 작성 등)
                        }
                    }
                }
            });
        }
        public static void UseItemNoti(Player player, Item item)
        {
            if (player == null || item == null)
                return;

            ItemDb itemDb = new ItemDb()
            {
                ItemDbId = item.ItemDbId,
                Count = item.Count,
            };

            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(ItemDb.Count)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (!success)
                    {
                        // 실패 시 로직 추가 (로그 작성 등)
                    }
                }
            });
        }
    }
}
