using CSharpAPI.Models;

namespace CSharpAPI.Service
{
    public interface IItemLineService
    {
        IEnumerable<ItemLineModel> GetAllItemLines();
        ItemLineModel GetItemLineById(int id);
        void CreateItemLine(ItemLineModel itemLine);
        bool UpdateItemLine(int id, ItemLineModel itemLine);
        bool DeleteItemLine(int id);
    }

    public class ItemLineService : IItemLineService
    {
        private List<ItemLineModel> _itemLines;
        private int _nextId = 1;

        public ItemLineService()
        {
            _itemLines = new List<ItemLineModel>
            {
                new ItemLineModel
                {
                    id = 0,
                    name = "Default Line",
                    description = "Default Line Description",
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
            _nextId = 1;
        }

        public IEnumerable<ItemLineModel> GetAllItemLines() => _itemLines;

        public ItemLineModel GetItemLineById(int id)
        {
            var itemLine = _itemLines.FirstOrDefault(x => x.id == id);
            if (itemLine == null)
            {
                throw new Exception($"ItemLine with id {id} not found");
            }
            return itemLine;
        }

        public void CreateItemLine(ItemLineModel itemLine)
        {
            itemLine.id = _nextId++;
            itemLine.created_at = DateTime.UtcNow;
            itemLine.updated_at = DateTime.UtcNow;
            _itemLines.Add(itemLine);
        }

        public bool UpdateItemLine(int id, ItemLineModel itemLine)
        {
            var existingItemLine = _itemLines.FirstOrDefault(x => x.id == id);
            if (existingItemLine == null) return false;

            existingItemLine.name = itemLine.name;
            existingItemLine.description = itemLine.description;
            existingItemLine.updated_at = DateTime.UtcNow;

            return true;
        }

        public bool DeleteItemLine(int id)
        {
            var itemLine = _itemLines.FirstOrDefault(x => x.id == id);
            if (itemLine == null) return false;

            _itemLines.Remove(itemLine);
            return true;
        }

    }
}