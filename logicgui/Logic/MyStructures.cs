using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace logic
{
	internal class MyStructures
	{

	}
	
	internal class Hashtable<T>
	{
		public string[] Keys
		{
			get 
			{
				var collection = new Stack<string>();
				foreach (var current in _records)
				{
					if (current==null) continue;
					var entry = current;
					while (entry != null)
					{
						collection.Push(entry.kvp.Key);
						entry = entry.next;
					}
				}
				
				return collection.ToArray();
			}
		}
		struct KeyValuePair
		{
			public T Value;
			public string Key;
		}
		class LinkedListItem
		{
			public KeyValuePair kvp;
			public LinkedListItem next;
		}
		private LinkedListItem[] _records;

		public Hashtable()
		{
			_records=new LinkedListItem[100];
		}
		public int Hash(string key)
		{
			int idx = 0;
			for (int i = 0; i < key.Length; i++)
				idx += key[i];
			return idx%_records.Length;
		}
		public void Add(string key, T val)
		{
			var idx = Hash(key);
			var current = _records[idx];
			while (current != null && current.kvp.Key != key) current = current.next;

			if (current == null)
			{
				var newitem = new LinkedListItem
				{
					kvp = new KeyValuePair { Key = key, Value = val },
					next = _records[idx]
				}; _records[idx] = newitem;
			}
			else current.kvp.Value = val;
		
		}
		public bool ContainsKey(string key)
		{
			var idx = Hash(key);
			var current = _records[idx];
			while (current != null && current.kvp.Key != key) current = current.next;
			if (current == null) return false;
			else return true;

		}
		public bool TryGetValue(string key, out T val)
		{
			var idx = Hash(key);
			var current = _records[idx];
			while (current != null && current.kvp.Key != key) current = current.next;
			if (current == null) { val = default;  return false; }
			else { val = current.kvp.Value;  return true; }
		}
		


	}
		internal class Stack<T> //: IEnumerable<>
	{
		private class Node
		{
			public T data;
			public Node link;
		}
		Node top;
		public Stack()
		{
			this.top = null;
			Count = 0;
		}
		public void Push(T item)
		{
			if (top == null) { top = new Node() { data = item, link = null }; }
			else { Node temp = new Node();
				//if (temp == null) return;
				temp.data = item;
				temp.link = top;
				top = temp;
				 }
			Count++;
		}
		public T Pop()
		{
			if (top==null) return default;
			var temp = top.data;
			top = top.link;
			Count--;
			return temp;
			
		}
		public T Peek()
		{
			if (top == null) return default;
			return top.data;
		}
		public T[] ToArray()
		{
			T[] result = new T[this.Count];
			Node current = this.top;
			for (int i = 0; i < this.Count; i++)
			{
				result[i] = current.data;
				current = current.link;
			}
			return result;
		}
		public Stack(T[] array)
		{
			for (int i = 0; i < array.Length; i++)
			{
				this.Push(array[i]); Count++;
			}
			this.Count = array.Length;
		}
	   // IEnumerator<T> IEnumerable<T>.GetEnumerator()
		//{
		//    throw new NotImplementedException();
		//}

		//IEnumerator IEnumerable.GetEnumerator()
		//{
		//    throw new NotImplementedException();
		//}

		public int Count
		{
			get; private set;
		}

		//public Stack (Stack<T> source)
		//{
		//    //for ()
		//    var temp = source.top;
		//    while (temp != null) { this.Push(temp.data); temp = temp.link; Count++; }

		//    //foreach (var item in source) { this.Push(item); Count++; }
		//}
	}
}
