﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using OSM.DBClass;

namespace OSM
{
    public partial class OSM_Order_Delivery_Form : UserControl
    {
        //父窗口
        private FormOSM_Main main_form;

        /// <summary>
        /// 构造函数
        /// </summary>
        public OSM_Order_Delivery_Form()
        {
            InitializeComponent();

            dateTimePicker_OrderDate.Text = DateTime.Now.ToString("yyyy-MM-dd");

            string whereString = "where PID = 2 order by ID";
            SJZDController.setZD_ComboBox(whereString, comboBox_OrderState);
            comboBox_OrderState.SelectedIndex = 0;
            comboBox_OrderState.Enabled = false;

            whereString = "where PID = 24 order by ID";
            SJZDController.setZD_ComboBox(whereString, comboBox_PayState);

            //查询订单视图返回结果
            queryFromOrderSheetV(dataGridView_Order,"where ORDER_STATE = '1' ");
        }

        /// <summary>
        /// 设置父窗口
        /// </summary>
        /// <param name="main_form"></param>
        public void setMainForm(FormOSM_Main mainForm)
        {
            main_form = mainForm;
        }

        /// <summary>
        /// 查询订单视图返回结果给DataGridView
        /// </summary>
        /// <param name="dgv"></param>
        /// <param name="whereString"></param>
        private void queryFromOrderSheetV(DataGridView dgv, string whereString = "")
        {
            string sql = "select * from OSM_ORDER_SHEET_V ";

            OSM_Pager_Order.setDataGridView(dataGridView_Order);
            OSM_Pager_Order.setQueryString(sql);
            OSM_Pager_Order.setKey("ID");
            OSM_Pager_Order.setWhereString(whereString);
            OSM_Pager_Order.setOrderString("ID DESC");
            OSM_Pager_Order.setPageIndex(1);
            OSM_Pager_Order.setPageSize(10);
            OSM_Pager_Order.refreshDataGirdViewBySQL(dataGridView_Order);
        }


        /// <summary>
        /// 重置按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Reset_Click(object sender, EventArgs e)
        {
            comboBox_OrderState.SelectedIndex = 0;
            comboBox_PayState.SelectedIndex = -1;
            dateTimePicker_OrderDate.CustomFormat = " ";
        }

        /// <summary>
        /// 日期控件下拉事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateTimePicker_OrderDate_DropDown(object sender, EventArgs e)
        {
            dateTimePicker_OrderDate.CustomFormat = "yyyy-MM-dd";
        }

        /// <summary>
        /// 查询按钮点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button_Query_Click(object sender, EventArgs e)
        {
            string whereString = "where ORDER_STATE = '1' ";

            if (!string.IsNullOrWhiteSpace(dateTimePicker_OrderDate.Text))
            {
                whereString += " and ORDER_DATE = #" + dateTimePicker_OrderDate.Text + "# ";
            }

            //if (comboBox_OrderState.SelectedIndex != -1)
            //{
            //    KeyValuePair<string, string> kv = (KeyValuePair<string, string>)comboBox_OrderState.SelectedItem;
            //    whereString += " and ORDER_STATE = '" + kv.Key + "'";
            //}

            if (comboBox_PayState.SelectedIndex != -1)
            {
                KeyValuePair<string, string> kv = (KeyValuePair<string, string>)comboBox_PayState.SelectedItem;
                whereString += " and PAY_STATE = '" + kv.Key + "' ";
            }

            queryFromOrderSheetV(dataGridView_Order, whereString);
        }

        /// <summary>
        /// DataGridView单元格点击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView_Order_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView_Order.Columns[e.ColumnIndex].Name == "viewOrderBtn")
            {
                string order_id = dataGridView_Order.Rows[e.RowIndex].Cells["ID"].Value.ToString();

                viewOrderSheet(order_id);
            }

            if (dataGridView_Order.Columns[e.ColumnIndex].Name == "auditBtn")
            {
                string order_id = dataGridView_Order.Rows[e.RowIndex].Cells["ID"].Value.ToString();
                string offersheet_code = dataGridView_Order.Rows[e.RowIndex].Cells["OFFERSHEET_CODE"].Value.ToString();

                checkOfferHW(order_id, offersheet_code);
            }
        }

        /// <summary>
        /// 查看订单按钮
        /// </summary>
        /// <param name="order_id"></param>
        private void viewOrderSheet(string order_id)
        {
            string sql = "select OSM_ORDER_SHEET.ID,OFFERSHEET_ID,ORDER_STATE,ORDER_DATE,PAY_MODE,PAY_STATE,ALREADY_PAYMENT,PRE_PAYMENT,REQUIRE_PAYMENT,OFFERSHEET_CODE ";
            sql += " from (OSM_ORDER_SHEET inner join OSM_OFFER_SHEET on OSM_ORDER_SHEET.OFFERSHEET_ID = OSM_OFFER_SHEET.ID) ";
            sql += " where OSM_ORDER_SHEET.ID = " + order_id + " ";

            AccessDB adb = new AccessDB();
            DataTable dt = adb.SQLTableQuery(sql);

            Hashtable hashtable = new Hashtable();

            if (dt.Rows.Count == 1)
            {
                DataRow dr = dt.Rows[0];
                hashtable.Add("ID", dr["ID"]);
                hashtable.Add("OFFERSHEET_ID", dr["OFFERSHEET_ID"]);
                hashtable.Add("ORDER_STATE", dr["ORDER_STATE"]);
                hashtable.Add("ORDER_DATE", dr["ORDER_DATE"]);
                hashtable.Add("PAY_MODE", dr["PAY_MODE"]);
                hashtable.Add("PAY_STATE", dr["PAY_STATE"]);
                hashtable.Add("ALREADY_PAYMENT", dr["ALREADY_PAYMENT"]);
                hashtable.Add("PRE_PAYMENT", dr["PRE_PAYMENT"]);
                hashtable.Add("REQUIRE_PAYMENT", dr["REQUIRE_PAYMENT"]);
                hashtable.Add("OFFERSHEET_CODE", dr["OFFERSHEET_CODE"]);
            }

            FormOSM_Order_View orderView = new FormOSM_Order_View();
            orderView.setOrderID(order_id);
            //设置窗口状态为查看
            orderView.setViewState(0);
            orderView.viewOrderSheet(hashtable);
            orderView.StartPosition = FormStartPosition.CenterParent;
            orderView.ShowDialog();
        }

        /// <summary>
        /// 检查订单对应货物列表
        /// </summary>
        /// <param name="order_id">订单ID</param>
        /// <param name="offersheet_code">报价单编号</param>
        private void checkOfferHW(string order_id, string offersheet_code)
        {
            string sql = "select PRODUCT_ID,HW_NUMBER from OSM_HW where OFFERSHEET_CODE = '" + offersheet_code + "'";

            AccessDB adb = new AccessDB();
            DataTable dt = adb.SQLTableQuery(sql);

            List<Hashtable> hashList = new List<Hashtable>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                Hashtable ht = new Hashtable();

                ht.Add("PRODUCT_ID", dr["PRODUCT_ID"]);
                //ht.Add("HW_NAME", dr["HW_NAME"]);
                //ht.Add("HW_CODE", dr["HW_CODE"]);
                ht.Add("HW_NUMBER", dr["HW_NUMBER"]);

                hashList.Add(ht);
            }

            if (checkDelivery(hashList, order_id))
            {
                //MessageBox.Show("根据与库存容量比对，可以发货！", "消息");
                sql = "update OSM_ORDER_SHEET set ORDER_STATE = '3' where ID = " + order_id;
                if (adb.SQLExecute(sql))
                {
                    MessageBox.Show("库存充足,对应订单已可以进行发货审核！", "消息");
                    //查询订单视图返回结果
                    queryFromOrderSheetV(dataGridView_Order, "where ORDER_STATE = '1' ");
                }
            }
            else
            {
                sql = "update OSM_ORDER_SHEET set ORDER_STATE = '2' where ID = " + order_id;
                if (adb.SQLExecute(sql))
                {
                    //查询订单视图返回结果
                    queryFromOrderSheetV(dataGridView_Order, "where ORDER_STATE = '1' ");
                }       
            }
        }

        /// <summary>
        /// 检查是否可以发货，不能发货则生成对应的采购单
        /// </summary>
        /// <param name="hashList">货物列表</param>
        /// <param name="order_id">订单ID</param>
        /// <returns></returns>
        private bool checkDelivery(List<Hashtable> hashList, string order_id)
        {
            List<Hashtable> purchaseList = new List<Hashtable>();
            AccessDB adb = new AccessDB();

            for (int i = 0; i < hashList.Count; i++)
            {
                Hashtable ht = hashList[i];
                string product_id = ht["PRODUCT_ID"].ToString();
                int hw_need_number = int.Parse(ht["HW_NUMBER"].ToString());

                string sql = "select ID,HW_NUMBER from OSM_STORAGE where ID = " + product_id + "";
                DataTable dt = adb.SQLTableQuery(sql);
                if (dt.Rows.Count == 1)
                {
                    DataRow dr = dt.Rows[0];
                    int hw_storage_number = int.Parse(dr["HW_NUMBER"].ToString());
                    int hw_diff_number = hw_need_number - hw_storage_number;
                    if (hw_diff_number > 0)
                    {
                        string updateSQL = "update OSM_STORAGE set HW_NUMBER = 0 where ID = " + product_id;
                        adb.SQLExecute(updateSQL);

                        ht["HW_NUMBER"] = hw_diff_number;
                        purchaseList.Add(ht);
                    }
                    else
                    {
                        string updateSQL = "update OSM_STORAGE set HW_NUMBER = " + (-hw_diff_number) + " where ID = " + product_id;
                        adb.SQLExecute(updateSQL);

                        break;
                    }
                }
                else
                {
                    ht["HW_NUMBER"] = hw_need_number;
                    purchaseList.Add(ht);
                }
            }

            if (purchaseList.Count == 0)
            {
                //string insertSQL = "insert into OSM_DILIVERY_SHEET(ORDER_ID,DILIVERY_STATE) values(" + order_id + ",'1')";
                //if (adb.SQLExecute(insertSQL))
                //{
                //    return true;
                //}
                return true;
            }
            else
            {
                int count = createPurchaseSheet(purchaseList, order_id);
                MessageBox.Show("库存不足，新增" + count + "条采购记录", "消息");
                return false;
            }
        }


        /// <summary>
        /// 根据得到的hashList货物列表生成采购单
        /// </summary>
        /// <param name="purchaseList">采购货物列表</param>
        /// <param name="order_id">订单</param>
        /// <returns>新增采购记录数量</returns>        
        private int createPurchaseSheet(List<Hashtable> purchaseList, string order_id)
        {
            AccessDB adb = new AccessDB();
            int count = 0;
            string insertSql = "insert into OSM_PURCHASE_SHEET(ORDER_ID,PRODUCT_ID,PURCHASE_NUMBER,PURCHASE_STATE,INIT_DATE) values (";

            for (int i = 0; i < purchaseList.Count; i++)
            {
                Hashtable ht = purchaseList[i];
                string valueString = order_id + ",";
                valueString +=  ht["PRODUCT_ID"].ToString() + ",";
                //valueString += "'" + ht["HW_NAME"].ToString() + "',";
                //valueString += "'" + ht["HW_CODE"].ToString() + "',";
                valueString += ht["HW_NUMBER"].ToString() + ",'1',#" + DateTime.Now.ToString("yyyy-MM-dd") + "#)";

                string sql = insertSql + valueString;
                if (adb.SQLExecute(sql))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
