using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private Dictionary<InventoryItemData, InventoryItem> itemDictionary;
    public List<InventoryItem> inventory { get; private set; }

    public static InventorySystem _instance;

    public List<Item> itemsToSave = new List<Item>();

    public System.Action onInventoryChangedEvent;

    [SerializeField] private GameObject _inventoryMenu;

    public bool _isActive = false;

    [System.Serializable]
    public struct Item
    {
        public string id;
        public string displayName;
        public int stackSize;
    }

    public void Save()
    {
        itemsToSave.Clear();

        foreach (var item in itemDictionary)
        {
            Item newItem = new Item();
            newItem.id = item.Key.id;
            newItem.displayName = item.Key.displayName;
            newItem.stackSize = item.Value.stackSize;

            itemsToSave.Add(newItem);
        }

        SaveSystem.Save(itemsToSave, "Inventory");
    }

    public void Load()
    {
        if (SaveSystem.SaveExists("Inventory"))
        {
            itemsToSave = SaveSystem.Load<List<Item>>("Inventory");

            foreach (var item in itemsToSave)
            {
                InventoryItemData newData = new InventoryItemData();
                newData.id = item.id;
                newData.displayName = item.displayName;
                newData.name = item.id;

                Add(newData, item.stackSize);
            }
        }
    }

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(this);
        
        inventory = new List<InventoryItem>();
        itemDictionary = new Dictionary<InventoryItemData, InventoryItem>();
        GameEvents.SaveInitiated += Save;
        GameEvents.LoadInitiated += Load;
    }

    private void Update()
    {
        if (!_isActive)
            return;

        if (Input.GetKeyDown(KeyCode.I))
            _inventoryMenu.SetActive(!_inventoryMenu.activeInHierarchy);
    }

    public bool CheckIfInventoryContains(InventoryItemData reference)
    {
        foreach (var item in inventory)
        {
            if (item.data.id == reference.id)
            {
                return true;
            }
        }

        return false;
    }

    public void Add (InventoryItemData referenceData, int stackSize = 1)
    {
        if (itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.AddToStack(stackSize);
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventory.Add(newItem);
            itemDictionary.Add(referenceData, newItem);

            if (stackSize >= 2)
                newItem.AddToStack(stackSize - 1);
        }

        onInventoryChangedEvent?.Invoke();
    }

    public void Remove(InventoryItemData referenceData)
    {
        if (itemDictionary.TryGetValue(referenceData, out InventoryItem value))
        {
            value.RemoveFromStack();

            if (value.stackSize <= 0)
            {
                inventory.Remove(value);

                itemDictionary.Remove(referenceData);
            }
        }
        else
        {
            InventoryItem newItem = new InventoryItem(referenceData);
            inventory.Remove(newItem);
            itemDictionary.Remove(referenceData);
        }

        onInventoryChangedEvent?.Invoke();
    }
}