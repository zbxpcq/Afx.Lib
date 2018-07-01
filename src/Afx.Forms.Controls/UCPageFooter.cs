using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Afx.Forms.Controls
{
    public partial class UCPageFooter : AfxBaseUserControl
    {
        public UCPageFooter()
        {
            InitializeComponent();

            this.SetPageSize(DefaultPageSizeList, 15);
            this.SetPageIndex(1, 0);
        }
        
        private static List<int> DefaultPageSizeList = new List<int>() { 10, 15, 20, 30, 50, 100 };

        private List<int> _pageSizeList = null;
        public List<int> PageSizeList
        {
            get
            {
                return new List<int>(this._pageSizeList ?? DefaultPageSizeList);
            }
        }

        private int _pageSize = 15;
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }
        }

        public void SetPageSize(List<int> pageSizeList, int pageSize)
        {
            if (pageSizeList == null || pageSizeList.Count == 0)
            {
                pageSizeList = DefaultPageSizeList;
            }
            this._pageSizeList = new List<int>(pageSizeList);
            this._pageSize = pageSize;
            List<AfxDropDownItem> list = new List<AfxDropDownItem>(this._pageSizeList.Count);
            foreach (var item in this._pageSizeList)
            {
                list.Add(new AfxDropDownItem() { Text = item.ToString(), Value = item });
            }
            this.dropDownList_PageSize.SelectedValueChanged -= dropDownList_PageSize_SelectedValueChanged;
            this.dropDownList_PageSize.DataSource = list;
            int i = this._pageSizeList.FindIndex(q => q == this._pageSize);
            if (i < 0)
            {
                i = this._pageSizeList.FindIndex(q => q == 15);
                if (i < 0) i = 0;
                this._pageSize = this._pageSizeList[i];
            }
            this.dropDownList_PageSize.SelectedIndex = i;
            this.dropDownList_PageSize.SelectedValueChanged += dropDownList_PageSize_SelectedValueChanged;
        }

        private int pageIndex = 1;
        public int PageIndex
        {
            get
            {
                return this.pageIndex;
            }
        }

        private int totalPage = 1;
        private int totalCount = 0;
        public int TotalCount
        {
            get
            {
                return this.totalCount;
            }
        }

        public void SetPageIndex(int pageIndex, int totalCount)
        {
            this.pageIndex = pageIndex < 1 ? 1 : pageIndex;
            this.totalCount = totalCount < 0 ? 0 : totalCount;
            this.SetPageText();
            this.SetEnabled();
        }

        private void SetPageText()
        {
            this.totalPage = 1;
            if (this._pageSize > 0)
            {
                this.totalPage = this.totalCount / this._pageSize;
                if (this.totalCount % this._pageSize > 0) this.totalPage++;
            }
            this.lb_PageCount.Text = string.Format("{0}/{1} 页", this.pageIndex, this.totalPage);
        }

        public event EventHandler MemberValueChanged;
        private void OnMemberValueChanged()
        {
            this.SetPageText();
            if(this.MemberValueChanged != null)
            {
                this.MemberValueChanged(this, EventArgs.Empty);
            }
        }

        private void SetEnabled()
        {
            if(this.pageIndex == 1 && this.totalPage == 1)
            {
                this.lbl_First.Enabled = false;
                this.lbl_Previous.Enabled = false;
                this.lbl_Next.Enabled = false;
                this.lbl_Last.Enabled = false;
            }
            else if (this.pageIndex == 1 && this.totalPage > 1)
            {
                this.lbl_First.Enabled = false;
                this.lbl_Previous.Enabled = false;
                this.lbl_Next.Enabled = true;
                this.lbl_Last.Enabled = true;
            }
            else if (this.totalPage > 1 && this.pageIndex == this.totalPage)
            {
                this.lbl_First.Enabled = true;
                this.lbl_Previous.Enabled = true;
                this.lbl_Next.Enabled = false;
                this.lbl_Last.Enabled = false;
            }
            else
            {
                this.lbl_First.Enabled = true;
                this.lbl_Previous.Enabled = true;
                this.lbl_Next.Enabled = true;
                this.lbl_Last.Enabled = true;
            }
        }
        
        private void dropDownList_PageSize_SelectedValueChanged(object sender, EventArgs e)
        {
            this._pageSize = (int)this.dropDownList_PageSize.SelectedValue;
            this.pageIndex = 1;
            this.SetEnabled();
            this.OnMemberValueChanged();
        }

        private void lbl_First_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.pageIndex = 1;
            this.SetEnabled();
            this.OnMemberValueChanged();
        }

        private void lbl_Previous_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if(this.pageIndex > 1)
            {
                this.pageIndex--;
                this.SetEnabled();
                this.OnMemberValueChanged();
            }
        }

        private void lbl_Next_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (this.pageIndex < this.totalPage)
            {
                this.pageIndex++;
                this.SetEnabled();
                this.OnMemberValueChanged();
            }
        }

        private void lbl_Last_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.pageIndex = this.totalPage;
            this.SetEnabled();
            this.OnMemberValueChanged();
        }

        
    }
}
