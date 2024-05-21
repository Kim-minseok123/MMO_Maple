using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager 
{
    public Dictionary<int, Item> Items { get; } = new Dictionary<int, Item>();
    public Item[] EquipItems { get; } = new Item[8];
    public void Add(Item item)
    {
        Items.Add(item.ItemDbId, item);
    }
    public Item Get(int itemDbId)
    {
        Item item = null;
        Items.TryGetValue(itemDbId, out item);
        return item;
    }
    public Item Find(Func<Item, bool> condition)
    {
        foreach (Item item in Items.Values) {
            if (condition.Invoke(item))
            {
                return item;
            }
        }
        return null;
    }
    public void Clear()
    {
        Items.Clear();
    }
    public void Remove(Item item)
    {
        Items.Remove(item.ItemDbId);
    }
    public void EquipAdd(int i, Item item)
    {
        EquipItems[i] = item;
    }
    public Item EquipGet(int i)
    {
        if (EquipItems[i] == null)
            return null;
        return EquipItems[i];
    }
    
    public void EquipClear()
    {
        for (int i = 0; i < EquipItems.Length; i++)
        {
            EquipItems[i] = null;
        }
    }
    public void EquipRemove(int i)
    {
        EquipItems[i] = null;
    }
}