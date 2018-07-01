using System;
using System.Collections.Generic;
using System.Text;

namespace Afx.Configuration
{
    class NodeModel
    {
        public string Name { get; set; }

        public List<ItemModel> Items { get; set; }

        public StringBuilder Comments { get; set; }

        public NodeModel()
        {
            this.Name = null;
            this.Comments = null;
            this.Items = new List<ItemModel>();
        }
    }
}
