using System.Collections.Generic;

namespace VisualMusic
{
    internal class UndoItems
    {
        public class Item
        {
            public string Desc;
            public Project Project;
            public Item(string desc, Project project)
            {
                Desc = desc;
                Project = project;
            }
        }

        LinkedList<Item> _items = new LinkedList<Item>();
        LinkedListNode<Item> _currentItem;
        LinkedListNode<Item> _savedItem;

        public Item Current => _currentItem == null ? null : _currentItem.Value;
        public Item Previous => _currentItem?.Previous == null ? null : _currentItem.Previous.Value;
        public Item Next => _currentItem?.Next == null ? null : _currentItem.Next.Value;
        public string UndoDesc => Current == null ? "" : Current.Desc;
        public string RedoDesc => Next == null ? "" : Next.Desc;
        public bool IsCurrentSaved => _currentItem != null && _currentItem == _savedItem;

        public void Clear()
        {
            foreach (var item in _items)
                item.Project.Dispose();
            _items.Clear();
            _currentItem = null;
            _savedItem = null;
        }

        public void MarkSaved()
        {
            _savedItem = _currentItem;
        }

        public void Add(string desc, Project project)
        {
            var snapshot = new Item(desc, project.Clone());

            if (_currentItem == null)
            {
                _currentItem = _items.AddLast(snapshot);
                return;
            }

            var node = _currentItem.Next;
            while (node != null)
            {
                var next = node.Next;
                node.Value.Project.Dispose();
                if (node == _savedItem)
                    _savedItem = null;
                _items.Remove(node);
                node = next;
            }

            _currentItem = _items.AddAfter(_currentItem, snapshot);
        }

        void ReplaceLast(string desc, Project project)
        {

        }

        public static UndoItems operator ++(UndoItems obj)
        {
            if (obj._currentItem != null)
                obj._currentItem = obj._currentItem.Next;
            return obj;
        }
        public static UndoItems operator --(UndoItems obj)
        {
            if (obj._currentItem != null)
                obj._currentItem = obj._currentItem.Previous;
            return obj;
        }
    }
}
