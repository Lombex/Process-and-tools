using CSharpAPI.Models;

namespace CSharpAPI.Service
{
    public interface IItemLineService
    {
        IEnumerable<ItemLine> GetAllItemLines();
        ItemLine GetItemLineById(int id);
        void CreateItemLine(ItemLine itemLine);
        bool UpdateItemLine(int id, ItemLine itemLine);
        bool DeleteItemLine(int id);
        IEnumerable<ItemLine> GetItemLinesByGroupId(int groupId);
    }

    public class ItemLineService : IItemLineService
    {
        private List<ItemLine> _itemLines;
        private int _nextId = 1;

        public ItemLineService()
        {
            _itemLines = new List<ItemLine>
            {
                new ItemLine
                {
                    id = 0,
                    name = "Default Line",
                    description = "Default Line Description",
                    itemgroup_id = 0,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                }
            };
            _nextId = 1;
        }

        public IEnumerable<ItemLine> GetAllItemLines() => _itemLines;

        public ItemLine GetItemLineById(int id)
        {
            var itemLine = _itemLines.FirstOrDefault(x => x.id == id);
            if (itemLine == null)
            {
                throw new Exception($"ItemLine with id {id} not found");
            }
            return itemLine;
        }

        public void CreateItemLine(ItemLine itemLine)
        {
            itemLine.id = _nextId++;
            itemLine.created_at = DateTime.UtcNow;
            itemLine.updated_at = DateTime.UtcNow;
            _itemLines.Add(itemLine);
        }

        public bool UpdateItemLine(int id, ItemLine itemLine)
        {
            var existingItemLine = _itemLines.FirstOrDefault(x => x.id == id);
            if (existingItemLine == null) return false;

            existingItemLine.name = itemLine.name;
            existingItemLine.description = itemLine.description;
            existingItemLine.itemgroup_id = itemLine.itemgroup_id;
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

        public IEnumerable<ItemLine> GetItemLinesByGroupId(int groupId)
        {
            return _itemLines.Where(l => l.itemgroup_id == groupId);
        }
    }
}