﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

public partial class newtallycasualimport : System.Web.UI.Page
{
    SqlCommand cmd;
    string userid = "";
    DBManager vdm;


    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["userid"] == null)
            Response.Redirect("Login.aspx");
        else
        {
            userid = Session["userid"].ToString();
            vdm = new DBManager();
            if (!Page.IsPostBack)
            {
                if (!Page.IsCallback)
                {
                    DateTime dtfrom = DateTime.Now.AddMonths(0);
                    DateTime dtyear = DateTime.Now.AddYears(0);
                    dtp_FromDate.Text = DateTime.Now.ToString("dd-MM-yyyy HH:mm");
                    bindbranchs();
                    string frmdate = dtfrom.ToString("dd/MM/yyyy");
                    string[] str = frmdate.Split('/');
                    ddlmonth.SelectedValue = str[1];
                    string fryear = dtyear.ToString("dd/MM/yyyy");
                    string[] str1 = fryear.Split('/');
                    ddlyear.SelectedValue = str1[2];
                }
            }
        }
    }

    private DateTime GetLowDate(DateTime dt)
    {
        double Hour, Min, Sec;
        DateTime DT = DateTime.Now;
        DT = dt;
        Hour = -dt.Hour;
        Min = -dt.Minute;
        Sec = -dt.Second;
        DT = DT.AddHours(Hour);
        DT = DT.AddMinutes(Min);
        DT = DT.AddSeconds(Sec);
        return DT;
    }

    private DateTime GetHighDate(DateTime dt)
    {
        double Hour, Min, Sec;
        DateTime DT = DateTime.Now;
        Hour = 23 - dt.Hour;
        Min = 59 - dt.Minute;
        Sec = 59 - dt.Second;
        DT = dt;
        DT = DT.AddHours(Hour);
        DT = DT.AddMinutes(Min);
        DT = DT.AddSeconds(Sec);
        return DT;
    }

    private void bindbranchs()
    {
        DBManager SalesDB = new DBManager();
        //branchwise
        //cmd = new SqlCommand("SELECT branchid, branchname,company_code FROM branchmaster where branchmaster.company_code='1'");
        string mainbranch = Session["mainbranch"].ToString();
        //branch mapping
        cmd = new SqlCommand("SELECT branchmaster.branchid, branchmaster.branchname, branchmaster.company_code FROM branchmaster INNER JOIN branchmapping ON branchmaster.branchid = branchmapping.subbranch WHERE (branchmapping.mainbranch = @m) ");
        cmd.Parameters.Add("@m", mainbranch);
        DataTable dttrips = vdm.SelectQuery(cmd).Tables[0];
        ddlbranch.DataSource = dttrips;
        ddlbranch.DataTextField = "branchname";
        ddlbranch.DataValueField = "branchid";
        ddlbranch.DataBind();
        ddlbranch.ClearSelection();
        ddlbranch.Items.Insert(0, new ListItem { Value = "0", Text = "--Select Branch--", Selected = true });
        ddlbranch.SelectedValue = "0";
    }
    protected void btn_Generate_Click(object sender, EventArgs e)
    {
        try
        {
            DataTable Report = new DataTable();
            DataTable nonpf = new DataTable();
            DateTime fromdate = DateTime.Now;
            DateTime mydate = DateTime.Now;
            string year = ddlyear.SelectedItem.Value;
            string mymonth = ddlmonth.SelectedItem.Value;
            string day = "";
            if (mymonth == "02")
            {
                day = "28";
            }
            else
            {
                day = (mydate.Day).ToString();
            }
            //string day = (mydate.Day).ToString();
            string date = mymonth + "/" + day + "/" + year;
            lblHeading.Text = ddlbranch.SelectedItem.Text + "Tally Casuals Salary Statement Report" + ddlmonth.SelectedItem.Text + " " + year;
            DateTime dtfrom = fromdate;
            string frmdate = dtfrom.ToString("dd/MM/yyyy");
            string[] str = frmdate.Split('/');
            lblFromDate.Text = mymonth;
            fromdate = Convert.ToDateTime(date);
            DateTime dt = Convert.ToDateTime(date);
            DateTime txttdate = DateTime.Now;
            string[] datestrig = dtp_FromDate.Text.Split(' ');
            if (datestrig.Length > 1)
            {
                if (datestrig[0].Split('-').Length > 0)
                {
                    string[] dates = datestrig[0].Split('-');
                    string[] times = datestrig[1].Split(':');
                    txttdate = new DateTime(int.Parse(dates[2]), int.Parse(dates[1]), int.Parse(dates[0]), int.Parse(times[0]), int.Parse(times[1]), 0);
                }
            }
            string talydate = txttdate.ToString("dd-MMM-yy");
            string talymonth = dt.ToString("MMM yy");
            Session["xporttype"] = "TallyCasuals";
            Session["filename"] = ddlbranch.SelectedItem.Text + "Tally Casuals Salary Statement " + ddlmonth.SelectedItem.Text + " " + year;
            Session["title"] = ddlbranch.SelectedItem.Text + "Tally Casuals Salary Statement " + ddlmonth.SelectedItem.Text + " " + year;
            string invoivebranch = "";
            cmd = new SqlCommand("SELECT company_master.address, company_master.companyname, branchmaster.branchcode FROM branchmaster INNER JOIN company_master ON branchmaster.company_code = company_master.sno WHERE (branchmaster.branchid = @BranchID)");
            cmd.Parameters.Add("@BranchID", ddlbranch.SelectedValue);
            DataTable dtcompany = vdm.SelectQuery(cmd).Tables[0];
            if (dtcompany.Rows.Count > 0)
            {
                lblAddress.Text = dtcompany.Rows[0]["address"].ToString();
                lblTitle.Text = dtcompany.Rows[0]["companyname"].ToString();
                invoivebranch = dtcompany.Rows[0]["branchcode"].ToString();
            }
            else
            {
                lblAddress.Text = Session["Address"].ToString();
                lblTitle.Text = Session["TitleName"].ToString();
            }

            string invoice = "" + invoivebranch + "JV" + mymonth + "";
            string mainbranch = Session["mainbranch"].ToString();
            if (mainbranch == "42")
            {
                if (ddlbranch.SelectedItem.Text != "SVF" || ddlbranch.SelectedItem.Text != "Wyra")
                {
                    Report.Columns.Add("SNO"); ;
                    Report.Columns.Add("Employee Code");
                    Report.Columns.Add("Employee Name");
                    Report.Columns.Add("ledgername");
                    Report.Columns.Add("ledgercode");
                    Report.Columns.Add("DESIGNATION");
                    Report.Columns.Add("GROSS").DataType = typeof(double);
                    Report.Columns.Add("Total Days In Month").DataType = typeof(double);
                    Report.Columns.Add("Payable Days").DataType = typeof(double);
                    Report.Columns.Add("AttendanceBonus>=24days@rs.30").DataType = typeof(double);
                    Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                    Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                    Report.Columns.Add("NET PAY").DataType = typeof(double);
                    Report.Columns.Add("Bank Acc NO");
                    Report.Columns.Add("IFSC Code");

                    nonpf.Columns.Add("SNO"); ;
                    nonpf.Columns.Add("Employee Code");
                    nonpf.Columns.Add("Employee Name");
                    nonpf.Columns.Add("ledgername");
                    nonpf.Columns.Add("ledgercode");
                    nonpf.Columns.Add("DESIGNATION");
                    nonpf.Columns.Add("GROSS").DataType = typeof(double);
                    nonpf.Columns.Add("Total Days In Month").DataType = typeof(double);
                    nonpf.Columns.Add("Payable Days").DataType = typeof(double);
                    nonpf.Columns.Add("AttendanceBonus>=24days@rs.30").DataType = typeof(double);
                    nonpf.Columns.Add("GROSS Earnings").DataType = typeof(double);
                    nonpf.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                    nonpf.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                    nonpf.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                    nonpf.Columns.Add("NET PAY").DataType = typeof(double);
                    nonpf.Columns.Add("Bank Acc NO");
                    nonpf.Columns.Add("IFSC Code");
                    int branchid = Convert.ToInt32(ddlbranch.SelectedItem.Value);
                    cmd = new SqlCommand("SELECT employebankdetails.ifsccode, employedetails.gender, employedetails.ledgercode, employedetails.ledgername,employedetails.pfeligible, employedetails.salarymode, employedetails.empid, employedetails.employee_num, employedetails.fullname, designation.designation, pay_structure.salaryperyear, employebankdetails.accountno, pay_structure.gross, monthly_attendance.month, monthly_attendance.year FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND ((employedetails.employee_type = 'Casuals') OR (employedetails.employee_type ='Casual worker')) AND employedetails.status='No' ORDER BY employedetails.gender");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.clorwo, monthly_attendance.month, monthly_attendance.year, monthly_attendance.extradays, employedetails.employee_num, branchmaster.company_code, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.sno, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN  branchmaster ON branchmaster.branchid = employedetails.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid)and (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, canteendeductions.amount, employedetails.fullname, employedetails.empid FROM  canteendeductions INNER JOIN employedetails ON canteendeductions.employee_num = employedetails.employee_num where (employedetails.branchid=@branchid) AND (canteendeductions.month = @month) AND (canteendeductions.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtcanteen = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, otherdeduction.otherdeductionamount FROM employedetails INNER JOIN otherdeduction ON employedetails.empid = otherdeduction.empid WHERE (employedetails.branchid = @branchid) AND (otherdeduction.month = @month) AND (otherdeduction.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtotherdeduction = vdm.SelectQuery(cmd).Tables[0];
                    if (dtsalary.Rows.Count > 0)
                    {
                        int i = 1;
                        foreach (DataRow dr in dtsalary.Rows)
                        {
                            double salaryadvance = 0;
                            double canteendeduction = 0;
                            double totalpresentdays = 0;
                            double totalearnings = 0;
                            double daysinmonth = 0;
                            double loseamount = 0;
                            double otherdeduction = 0;
                            DataRow newrow = Report.NewRow();
                            DataRow dtnewrow = nonpf.NewRow();
                            string pf = dr["pfeligible"].ToString();
                            if (pf == "Yes")
                            {
                                newrow["SNO"] = i++.ToString();
                                newrow["Employee Code"] = dr["employee_num"].ToString();
                                newrow["Employee Name"] = dr["fullname"].ToString();
                                newrow["ledgername"] = dr["ledgername"].ToString();
                                newrow["ledgercode"] = dr["ledgercode"].ToString();
                                double peryanam = Convert.ToDouble(dr["salaryperyear"].ToString());
                                newrow["GROSS"] = peryanam / 12;
                                double permonth = peryanam / 12;
                                newrow["Bank Acc NO"] = dr["accountno"].ToString();
                                newrow["IFSC Code"] = dr["ifsccode"].ToString();
                                Report.Rows.Add(newrow);
                                if (dtattendence.Rows.Count > 0)
                                {
                                    foreach (DataRow dra in dtattendence.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                    {

                                        double numberofworkingdays = 0;
                                        double.TryParse(dra["numberofworkingdays"].ToString(), out numberofworkingdays);
                                        double clorwo = 0;
                                        double.TryParse(dra["clorwo"].ToString(), out clorwo);
                                        daysinmonth = numberofworkingdays + clorwo;
                                        newrow["Total Days in Month"] = daysinmonth.ToString();
                                        //double rateperday = 0;
                                        double paydays = 0;
                                        double lop = 0;
                                        double.TryParse(dra["lop"].ToString(), out lop);
                                        paydays = numberofworkingdays - lop;
                                        double holidays = 0;
                                        holidays = daysinmonth - numberofworkingdays;
                                        newrow["Payable Days"] = paydays;
                                        string companycode = dra["company_code"].ToString();
                                        double rate = 0;
                                        double gr = Convert.ToDouble(dr["gross"].ToString());
                                        rate = gr;
                                        double bonus;
                                        string gender = dr["gender"].ToString();
                                        if (gender == "Male")
                                        {
                                            bonus = 30;
                                        }
                                        else
                                        {
                                            bonus = 35;
                                        }
                                        double rateperday = 0;
                                        double amount = 0;
                                        if (paydays >= 24)
                                        {
                                            rateperday = rate + bonus;
                                            amount = bonus * paydays;
                                            newrow["AttendanceBonus>=24days@rs.30"] = amount;
                                        }
                                        else
                                        {
                                            newrow["AttendanceBonus>=24days@rs.30"] = 0;
                                            rate = gr;
                                        }
                                        totalearnings = rate * paydays + amount;
                                        double totalpdays = numberofworkingdays - lop;
                                        totalearnings = Math.Round(totalearnings);
                                    }
                                    canteendeduction = 0;
                                    if (dtcanteen.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drcanteen in dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double deductionamount = 0;
                                                double.TryParse(drcanteen["amount"].ToString(), out deductionamount);
                                                newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                                string st = deductionamount.ToString();
                                                canteendeduction = Convert.ToDouble(deductionamount);
                                                canteendeduction = Math.Round(deductionamount);
                                            }
                                        }
                                        else
                                        {
                                            canteendeduction = 0;
                                            newrow["CANTEEN DEDUCTION"] = canteendeduction;
                                        }
                                    }
                                    otherdeduction = 0;
                                    if (dtotherdeduction.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drotherdeduction in dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double amount = 0;
                                                double.TryParse(drotherdeduction["otherdeductionamount"].ToString(), out amount);
                                                if (amount == 0)
                                                {
                                                }
                                                else
                                                {
                                                    newrow["OTHER DEDUCTION"] = amount.ToString();
                                                    string st = amount.ToString();
                                                    otherdeduction = Convert.ToDouble(amount);
                                                    otherdeduction = Math.Round(amount, 0);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            otherdeduction = 0;
                                            if (otherdeduction == 0)
                                            {
                                            }
                                            else
                                            {
                                                //otherdeduction = 0;
                                                newrow["OTHER DEDUCTION"] = otherdeduction;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        otherdeduction = 0;
                                        if (otherdeduction == 0)
                                        {
                                        }
                                        else
                                        {
                                            //otherdeduction = 0;
                                            newrow["OTHER DEDUCTION"] = otherdeduction;
                                        }
                                    }
                                    salaryadvance = 0;
                                    if (dtsa.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drsa in dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double amount = 0;
                                                double.TryParse(drsa["amount"].ToString(), out amount);
                                                newrow["SALARY ADVANCE"] = amount.ToString();
                                                salaryadvance = Convert.ToDouble(amount);
                                                salaryadvance = Math.Round(amount, 0);
                                            }
                                        }
                                        else
                                        {
                                            salaryadvance = 0;
                                            newrow["SALARY ADVANCE"] = salaryadvance;
                                        }
                                    }
                                }
                                newrow["GROSS Earnings"] = totalearnings;
                                double totaldd = salaryadvance + canteendeduction + otherdeduction;
                                double netpay = 0;
                                netpay = totalearnings - totaldd;
                                netpay = Math.Round(netpay, 2);
                                newrow["NET PAY"] = netpay;
                            }
                            else
                            {
                                dtnewrow["SNO"] = i++.ToString();
                                dtnewrow["Employee Code"] = dr["employee_num"].ToString();
                                dtnewrow["Employee Name"] = dr["fullname"].ToString();
                                dtnewrow["ledgername"] = dr["ledgername"].ToString();
                                dtnewrow["ledgercode"] = dr["ledgercode"].ToString();
                                double peryanam = Convert.ToDouble(dr["salaryperyear"].ToString());
                                dtnewrow["GROSS"] = peryanam / 12;
                                double permonth = peryanam / 12;
                                dtnewrow["Bank Acc NO"] = dr["accountno"].ToString();
                                dtnewrow["IFSC Code"] = dr["ifsccode"].ToString();
                                nonpf.Rows.Add(dtnewrow);
                                if (dtattendence.Rows.Count > 0)
                                {
                                    foreach (DataRow dra in dtattendence.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                    {

                                        double numberofworkingdays = 0;
                                        double.TryParse(dra["numberofworkingdays"].ToString(), out numberofworkingdays);
                                        double clorwo = 0;
                                        double.TryParse(dra["clorwo"].ToString(), out clorwo);
                                        daysinmonth = numberofworkingdays + clorwo;
                                        dtnewrow["Total Days in Month"] = daysinmonth.ToString();
                                        //double rateperday = 0;
                                        double paydays = 0;
                                        double lop = 0;
                                        double.TryParse(dra["lop"].ToString(), out lop);
                                        paydays = numberofworkingdays - lop;
                                        double holidays = 0;
                                        holidays = daysinmonth - numberofworkingdays;
                                        dtnewrow["Payable Days"] = paydays;
                                        string companycode = dra["company_code"].ToString();
                                        double rate = 0;
                                        double gr = Convert.ToDouble(dr["gross"].ToString());
                                        rate = gr;
                                        double bonus;
                                        string gender = dr["gender"].ToString();
                                        if (gender == "Male")
                                        {
                                            bonus = 30;
                                        }
                                        else
                                        {
                                            bonus = 35;
                                        }
                                        double rateperday = 0;
                                        double amount = 0;
                                        if (paydays >= 24)
                                        {
                                            rateperday = rate + bonus;
                                            amount = bonus * paydays;
                                            dtnewrow["AttendanceBonus>=24days@rs.30"] = amount;
                                        }
                                        else
                                        {
                                            dtnewrow["AttendanceBonus>=24days@rs.30"] = 0;
                                            rate = gr;
                                        }
                                        totalearnings = rate * paydays + amount;
                                        double totalpdays = numberofworkingdays - lop;
                                        totalearnings = Math.Round(totalearnings);
                                    }
                                    canteendeduction = 0;
                                    if (dtcanteen.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drcanteen in dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double deductionamount = 0;
                                                double.TryParse(drcanteen["amount"].ToString(), out deductionamount);
                                                dtnewrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                                string st = deductionamount.ToString();
                                                canteendeduction = Convert.ToDouble(deductionamount);
                                                canteendeduction = Math.Round(deductionamount);
                                            }
                                        }
                                        else
                                        {
                                            canteendeduction = 0;
                                            dtnewrow["CANTEEN DEDUCTION"] = canteendeduction;
                                        }
                                    }
                                    otherdeduction = 0;
                                    if (dtotherdeduction.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drotherdeduction in dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double amount = 0;
                                                double.TryParse(drotherdeduction["otherdeductionamount"].ToString(), out amount);
                                                if (amount == 0)
                                                {
                                                }
                                                else
                                                {
                                                    newrow["OTHER DEDUCTION"] = amount.ToString();
                                                    string st = amount.ToString();
                                                    otherdeduction = Convert.ToDouble(amount);
                                                    otherdeduction = Math.Round(amount, 0);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            otherdeduction = 0;
                                            if (otherdeduction == 0)
                                            {
                                            }
                                            else
                                            {
                                                //otherdeduction = 0;
                                                newrow["OTHER DEDUCTION"] = otherdeduction;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        otherdeduction = 0;
                                        if (otherdeduction == 0)
                                        {
                                        }
                                        else
                                        {
                                            //otherdeduction = 0;
                                            newrow["OTHER DEDUCTION"] = otherdeduction;
                                        }
                                    }
                                    salaryadvance = 0;
                                    if (dtsa.Rows.Count > 0)
                                    {
                                        DataRow[] drr = dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                        if (drr.Length > 0)
                                        {
                                            foreach (DataRow drsa in dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                            {
                                                double amount = 0;
                                                double.TryParse(drsa["amount"].ToString(), out amount);
                                                dtnewrow["SALARY ADVANCE"] = amount.ToString();
                                                salaryadvance = Convert.ToDouble(amount);
                                                salaryadvance = Math.Round(amount, 0);
                                            }
                                        }
                                        else
                                        {
                                            salaryadvance = 0;
                                            dtnewrow["SALARY ADVANCE"] = salaryadvance;
                                        }
                                    }
                                }
                                dtnewrow["GROSS Earnings"] = totalearnings;
                                double totaldd = salaryadvance + canteendeduction + otherdeduction;
                                double netpay = 0;
                                netpay = totalearnings - totaldd;
                                netpay = Math.Round(netpay, 2);
                                dtnewrow["NET PAY"] = Math.Round(netpay);
                            }
                        }
                    }
                }
                else
                {
                    Report.Columns.Add("SNO"); ;
                    Report.Columns.Add("Employee Code");
                    Report.Columns.Add("Employee Name");
                    Report.Columns.Add("ledgername");
                    Report.Columns.Add("ledgercode");
                    Report.Columns.Add("DESIGNATION");
                    Report.Columns.Add("GROSS").DataType = typeof(double);
                    Report.Columns.Add("Total Days In Month").DataType = typeof(double);
                    Report.Columns.Add("Payable Days").DataType = typeof(double);
                    Report.Columns.Add("AttendanceBonus>=24days@rs.30").DataType = typeof(double);
                    Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                    Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                    Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("NET PAY").DataType = typeof(double);
                    Report.Columns.Add("Bank Acc NO");
                    Report.Columns.Add("IFSC Code");
                }
            }
            else
            {
                if (ddlbranch.SelectedItem.Text != "RMRD plant" && ddlbranch.SelectedItem.Text != "Arani CC")
                {
                    Report.Columns.Add("SNO");
                    Report.Columns.Add("Employee Code");
                    Report.Columns.Add("Employee Name");
                    Report.Columns.Add("ledgername");
                    Report.Columns.Add("ledgercode");
                    Report.Columns.Add("DESIGNATION");
                    Report.Columns.Add("GROSS").DataType = typeof(double);
                    Report.Columns.Add("Total Days In Month").DataType = typeof(double);
                    Report.Columns.Add("Payable Days").DataType = typeof(double);
                    Report.Columns.Add("AttendanceBonus>=26days@rs.20").DataType = typeof(double);
                    Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                    Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                    Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("NET PAY").DataType = typeof(double);
                    Report.Columns.Add("Bank Acc NO");
                    Report.Columns.Add("IFSC Code");
                    int branchid = Convert.ToInt32(ddlbranch.SelectedItem.Value);
                    cmd = new SqlCommand("SELECT employebankdetails.ifsccode, employedetails.salarymode, employedetails.ledgercode, employedetails.ledgername,employedetails.empid, employedetails.employee_num, employedetails.fullname, designation.designation, salaryappraisals.salaryperyear, employebankdetails.accountno,salaryappraisals.gross, monthly_attendance.month, monthly_attendance.year FROM  employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN salaryappraisals ON employedetails.empid = salaryappraisals.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND (employedetails.employee_type = 'Casuals' OR employedetails.employee_type = 'Casual worker') AND (employedetails.status = 'No') AND (salaryappraisals.endingdate IS NULL) AND (salaryappraisals.startingdate <= @d1) OR (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND (employedetails.employee_type = 'Casuals' OR employedetails.employee_type = 'Casual worker') AND (employedetails.status = 'No') AND (salaryappraisals.endingdate > @d1) AND (salaryappraisals.startingdate <= @d1)ORDER BY employedetails.gender,employedetails.employee_num");
                    //cmd = new SqlCommand("SELECT employebankdetails.ifsccode, employedetails.salarymode, employedetails.ledgercode, employedetails.ledgername,employedetails.empid, employedetails.employee_num, employedetails.fullname, designation.designation, pay_structure.salaryperyear, employebankdetails.accountno, pay_structure.gross, monthly_attendance.month, monthly_attendance.year FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND ((employedetails.employee_type = 'Casuals') OR (employedetails.employee_type ='Casual worker')) AND employedetails.status='No'");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@d1", date);
                    DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.clorwo,monthly_attendance.month, monthly_attendance.year, monthly_attendance.extradays, employedetails.employee_num, branchmaster.company_code, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.sno, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN  branchmaster ON branchmaster.branchid = employedetails.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) and (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, canteendeductions.amount, employedetails.fullname, employedetails.empid FROM  canteendeductions INNER JOIN employedetails ON canteendeductions.employee_num = employedetails.employee_num where (employedetails.branchid=@branchid) AND (canteendeductions.month = @month) AND (canteendeductions.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtcanteen = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, otherdeduction.otherdeductionamount FROM employedetails INNER JOIN otherdeduction ON employedetails.empid = otherdeduction.empid WHERE (employedetails.branchid = @branchid) AND (otherdeduction.month = @month) AND (otherdeduction.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtotherdeduction = vdm.SelectQuery(cmd).Tables[0];
                    if (dtsalary.Rows.Count > 0)
                    {
                        int i = 1;

                        foreach (DataRow dr in dtsalary.Rows)
                        {
                            double salaryadvance = 0;
                            double canteendeduction = 0;
                            double totalpresentdays = 0;
                            double totalearnings = 0;
                            double daysinmonth = 0;
                            double loseamount = 0;
                            double otherdeduction = 0;
                            DataRow newrow = Report.NewRow();
                            newrow["SNO"] = i++.ToString();
                            newrow["Employee Code"] = dr["employee_num"].ToString();
                            newrow["Employee Name"] = dr["fullname"].ToString();
                            newrow["ledgername"] = dr["ledgername"].ToString();
                            newrow["ledgercode"] = dr["ledgercode"].ToString();
                            double peryanam = Convert.ToDouble(dr["salaryperyear"].ToString());
                            newrow["GROSS"] = peryanam / 12;
                            double permonth = peryanam / 12;
                            newrow["Bank Acc NO"] = dr["accountno"].ToString();
                            newrow["IFSC Code"] = dr["ifsccode"].ToString();
                            Report.Rows.Add(newrow);
                            if (dtattendence.Rows.Count > 0)
                            {
                                foreach (DataRow dra in dtattendence.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                {
                                    double numberofworkingdays = 0;
                                    double.TryParse(dra["numberofworkingdays"].ToString(), out numberofworkingdays);
                                    double clorwo = 0;
                                    double.TryParse(dra["clorwo"].ToString(), out clorwo);
                                    daysinmonth = numberofworkingdays + clorwo;
                                    newrow["Total Days in Month"] = daysinmonth.ToString();
                                    //double rateperday = 0;
                                    double paydays = 0;
                                    double lop = 0;
                                    double.TryParse(dra["lop"].ToString(), out lop);
                                    paydays = numberofworkingdays - lop;
                                    double holidays = 0;
                                    holidays = daysinmonth - numberofworkingdays;
                                    newrow["Payable Days"] = paydays;
                                    string companycode = dra["company_code"].ToString();
                                    double rate = 0;
                                    if (companycode == "2")
                                    {
                                        double gr = Convert.ToDouble(dr["gross"].ToString());
                                        rate = gr / daysinmonth;
                                    }
                                    else
                                    {
                                        if (ddlbranch.SelectedItem.Text == "Punabaka Plant" || ddlbranch.SelectedItem.Text == "PBK KKMC")
                                        {
                                            rate = Convert.ToDouble(dr["gross"].ToString());
                                        }
                                        else
                                        {
                                            double grr = Convert.ToDouble(dr["gross"].ToString());
                                            rate = grr / daysinmonth;
                                        }
                                    }
                                    // newrow["Rate/Day"] = rate;
                                    double bonus = 20;
                                    double rateperday = 0;
                                    double amount = 0;
                                    if (paydays >= 26)
                                    {
                                        if (ddlbranch.SelectedItem.Text == "Punabaka Plant")
                                        {
                                            rateperday = rate + bonus;
                                            amount = bonus * paydays;
                                            newrow["AttendanceBonus>=26days@rs.20"] = amount;
                                        }
                                        else
                                        {
                                            newrow["AttendanceBonus>=26days@rs.20"] = 0;
                                            rateperday = rate;
                                        }
                                    }
                                    else
                                    {
                                        newrow["AttendanceBonus>=26days@rs.20"] = 0;
                                        rateperday = rate + bonus;
                                    }
                                    totalearnings = rate * paydays + amount;
                                    double totalpdays = numberofworkingdays - lop;
                                    totalearnings = Math.Round(totalearnings);
                                }
                                canteendeduction = 0;
                                if (dtcanteen.Rows.Count > 0)
                                {
                                    DataRow[] drr = dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                    if (drr.Length > 0)
                                    {
                                        foreach (DataRow drcanteen in dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                        {
                                            double deductionamount = 0;
                                            double.TryParse(drcanteen["amount"].ToString(), out deductionamount);
                                            newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                            string st = deductionamount.ToString();
                                            canteendeduction = Convert.ToDouble(deductionamount);
                                            canteendeduction = Math.Round(deductionamount);
                                        }
                                    }
                                    else
                                    {
                                        canteendeduction = 0;
                                        newrow["CANTEEN DEDUCTION"] = canteendeduction;
                                    }
                                }
                                otherdeduction = 0;
                                if (dtotherdeduction.Rows.Count > 0)
                                {
                                    DataRow[] drr1 = dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                    if (drr1.Length > 0)
                                    {
                                        foreach (DataRow drotherdeduction in dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                        {
                                            double amount = 0;
                                            double.TryParse(drotherdeduction["otherdeductionamount"].ToString(), out amount);
                                            if (amount == 0)
                                            {
                                            }
                                            else
                                            {
                                                newrow["OTHER DEDUCTION"] = amount.ToString();
                                                string st = amount.ToString();
                                                otherdeduction = Convert.ToDouble(amount);
                                                otherdeduction = Math.Round(amount, 0);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        otherdeduction = 0;
                                        if (otherdeduction == 0)
                                        {
                                        }
                                        else
                                        {
                                            //otherdeduction = 0;
                                            newrow["OTHER DEDUCTION"] = otherdeduction;
                                        }
                                    }
                                }
                                else
                                {
                                    otherdeduction = 0;
                                    if (otherdeduction == 0)
                                    {
                                    }
                                    else
                                    {
                                        //otherdeduction = 0;
                                        newrow["OTHER DEDUCTION"] = otherdeduction;
                                    }
                                }

                                salaryadvance = 0;
                                if (dtsa.Rows.Count > 0)
                                {
                                    DataRow[] drr = dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                    if (drr.Length > 0)
                                    {
                                        foreach (DataRow drsa in dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                        {
                                            double amount = 0;
                                            double.TryParse(drsa["amount"].ToString(), out amount);
                                            newrow["SALARY ADVANCE"] = amount.ToString();
                                            salaryadvance = Convert.ToDouble(amount);
                                            salaryadvance = Math.Round(amount, 0);
                                        }
                                    }
                                    else
                                    {
                                        salaryadvance = 0;
                                        newrow["SALARY ADVANCE"] = salaryadvance;
                                    }
                                }
                            }
                            newrow["GROSS Earnings"] = totalearnings;
                            double totaldd = salaryadvance + canteendeduction + otherdeduction;
                            double netpay = 0;
                            netpay = totalearnings - totaldd;
                            netpay = Math.Round(netpay, 2);
                            newrow["NET PAY"] = netpay;
                        }
                    }
                }
                else if (ddlbranch.SelectedItem.Text == "Arani CC" || ddlbranch.SelectedItem.Text == "RMRD plant")
                {
                    double totalearnings = 0;
                    Report.Columns.Add("SNO");
                    Report.Columns.Add("Employeeid");
                    Report.Columns.Add("Employee Code");
                    Report.Columns.Add("Employee Name");
                    Report.Columns.Add("ledgername");
                    Report.Columns.Add("ledgercode");
                    Report.Columns.Add("DESIGNATION");
                    Report.Columns.Add("Rate/Day").DataType = typeof(double);
                    Report.Columns.Add("Total Days in Month").DataType = typeof(double);
                    Report.Columns.Add("Payable Days").DataType = typeof(double);
                    Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                    Report.Columns.Add("AttendanceBonus>=26days@rs.20").DataType = typeof(double);
                    Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                    Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                    Report.Columns.Add("NET PAY").DataType = typeof(double);
                    Report.Columns.Add("Bank Acc NO");
                    Report.Columns.Add("IFSC Code");
                    int branchid = Convert.ToInt32(ddlbranch.SelectedItem.Value);
                    cmd = new SqlCommand("SELECT employebankdetails.ifsccode, employedetails.salarymode, employedetails.ledgercode, employedetails.ledgername,employedetails.empid, employedetails.employee_num, employedetails.fullname, designation.designation, salaryappraisals.salaryperyear, employebankdetails.accountno,salaryappraisals.gross, monthly_attendance.month, monthly_attendance.year FROM  employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN salaryappraisals ON employedetails.empid = salaryappraisals.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND (employedetails.employee_type = 'Casuals' OR employedetails.employee_type = 'Casual worker') AND (employedetails.status = 'No') AND (salaryappraisals.endingdate IS NULL) AND (salaryappraisals.startingdate <= @d1) OR (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.salarymode = '1') AND (employedetails.employee_type = 'Casuals' OR employedetails.employee_type = 'Casual worker') AND (employedetails.status = 'No') AND (salaryappraisals.endingdate > @d1) AND (salaryappraisals.startingdate <= @d1)ORDER BY employedetails.gender,employedetails.employee_num");
                    // cmd = new SqlCommand("SELECT employedetails.pfeligible, employebankdetails.ifsccode, employedetails.ledgercode, employedetails.ledgername,employedetails.esieligible,employedetails.employee_type, employedetails.empid,employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.gross, pay_structure.hra, pay_structure.conveyance,  pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid  LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid=@branchid) and (employedetails.status='No') and (employedetails.employee_type='Casuals')");
                    cmd.Parameters.Add("@branchid", branchid);
                    //cmd.Parameters.Add("@emptype", employee_type);
                    DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid,monthly_attendance.clorwo, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN  employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid WHERE  (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                    //cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_workingdays.numberofworkingdays, monthly_attendance.lop,  branchmaster.branchid FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN monthly_workingdays ON branchmaster.branchid = monthly_workingdays.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid=@branchid)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) and  (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    cmd.Parameters.Add("@branchid", branchid);
                    DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT  employedetails.employee_num, canteendeductions.amount, employedetails.fullname, employedetails.empid FROM  canteendeductions INNER JOIN employedetails ON canteendeductions.employee_num = employedetails.employee_num where (employedetails.branchid=@branchid) AND (canteendeductions.month = @month) AND (canteendeductions.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtcanteen = vdm.SelectQuery(cmd).Tables[0];
                    cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, otherdeduction.otherdeductionamount FROM employedetails INNER JOIN otherdeduction ON employedetails.empid = otherdeduction.empid WHERE (employedetails.branchid = @branchid) AND (otherdeduction.month = @month) AND (otherdeduction.year = @year)");
                    cmd.Parameters.Add("@branchid", branchid);
                    cmd.Parameters.Add("@month", mymonth);
                    cmd.Parameters.Add("@year", year);
                    DataTable dtotherdeduction = vdm.SelectQuery(cmd).Tables[0];
                    if (dtsalary.Rows.Count > 0)
                    {
                        var i = 1;
                        foreach (DataRow dr in dtsalary.Rows)
                        {
                            double totalpresentdays = 0;
                            double profitionaltax = 0;
                            double salaryadvance = 0;
                            double loan = 0;
                            double canteendeduction = 0;
                            double mobilededuction = 0;
                            double totaldeduction;
                            double otherdeduction;
                            double daysinmonth = 0;
                            double loseamount = 0;
                            DataRow newrow = Report.NewRow();
                            newrow["SNO"] = i++.ToString();
                            newrow["Employeeid"] = dr["empid"].ToString();
                            newrow["Employee Code"] = dr["employee_num"].ToString();
                            newrow["Employee Name"] = dr["fullname"].ToString();
                            newrow["ledgername"] = dr["ledgername"].ToString();
                            newrow["ledgercode"] = dr["ledgercode"].ToString();
                            newrow["DESIGNATION"] = dr["designation"].ToString();
                            double BASIC = Convert.ToDouble(dr["erningbasic"].ToString());
                            //double rateper = Convert.ToDouble(dr["gross"].ToString());
                            //profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
                            newrow["Bank Acc NO"] = dr["accountno"].ToString();
                            newrow["IFSC Code"] = dr["ifsccode"].ToString();

                            if (dtattendence.Rows.Count > 0)
                            {
                                foreach (DataRow dra in dtattendence.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                {
                                    double numberofworkingdays = 0;
                                    double.TryParse(dra["numberofworkingdays"].ToString(), out numberofworkingdays);
                                    double clorwo = 0;
                                    double.TryParse(dra["clorwo"].ToString(), out clorwo);
                                    daysinmonth = numberofworkingdays + clorwo;
                                    newrow["Total Days in Month"] = daysinmonth.ToString();
                                    //double rateperday = 0;
                                    double paydays = 0;
                                    double lop = 0;
                                    double.TryParse(dra["lop"].ToString(), out lop);
                                    paydays = daysinmonth - lop;
                                    double holidays = 0;
                                    holidays = daysinmonth - numberofworkingdays;
                                    newrow["Payable Days"] = paydays;
                                    double rate = Convert.ToDouble(dr["gross"].ToString());
                                    newrow["Rate/Day"] = rate;
                                    if (paydays >= 26)
                                    {
                                        if (ddlbranch.SelectedItem.Text == "RMRD plant")
                                        {
                                            double bonus = 20;
                                            rate = rate + bonus;
                                            newrow["AttendanceBonus>=26days@rs.20"] = bonus * paydays;
                                        }
                                    }
                                    totalearnings = rate * paydays;
                                    double totalpdays = numberofworkingdays - lop;
                                    totalearnings = Math.Round(totalearnings);
                                    newrow["GROSS Earnings"] = totalearnings;
                                }
                            }
                            canteendeduction = 0;
                            if (dtcanteen.Rows.Count > 0)
                            {
                                DataRow[] drr = dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                if (drr.Length > 0)
                                {
                                    foreach (DataRow drcanteen in dtcanteen.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                    {
                                        double deductionamount = 0;
                                        double.TryParse(drcanteen["amount"].ToString(), out deductionamount);
                                        newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                        string st = deductionamount.ToString();
                                        canteendeduction = Convert.ToDouble(deductionamount);
                                        canteendeduction = Math.Ceiling(deductionamount);
                                    }
                                }
                                else
                                {
                                    canteendeduction = 0;
                                    newrow["CANTEEN DEDUCTION"] = canteendeduction;
                                }
                            }
                            otherdeduction = 0;
                            if (dtotherdeduction.Rows.Count > 0)
                            {
                                DataRow[] drr = dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                if (drr.Length > 0)
                                {
                                    foreach (DataRow drotherdeduction in dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                    {
                                        double amount = 0;
                                        double.TryParse(drotherdeduction["otherdeductionamount"].ToString(), out amount);
                                        if (amount == 0)
                                        {
                                        }
                                        else
                                        {
                                            newrow["OTHER DEDUCTION"] = amount.ToString();
                                            string st = amount.ToString();
                                            otherdeduction = Convert.ToDouble(amount);
                                            otherdeduction = Math.Round(amount, 0);
                                        }
                                    }
                                }
                                else
                                {
                                    otherdeduction = 0;
                                    if (otherdeduction == 0)
                                    {
                                    }
                                    else
                                    {
                                        //otherdeduction = 0;
                                        newrow["OTHER DEDUCTION"] = otherdeduction;
                                    }
                                }
                            }
                            else
                            {
                                otherdeduction = 0;
                                if (otherdeduction == 0)
                                {
                                }
                                else
                                {
                                    //otherdeduction = 0;
                                    newrow["OTHER DEDUCTION"] = otherdeduction;
                                }
                            }
                            salaryadvance = 0;
                            if (dtsa.Rows.Count > 0)
                            {
                                DataRow[] drr = dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                                if (drr.Length > 0)
                                {
                                    foreach (DataRow drsa in dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                    {
                                        double amount = 0;
                                        double.TryParse(drsa["amount"].ToString(), out amount);
                                        newrow["SALARY ADVANCE"] = amount.ToString();
                                        salaryadvance = Convert.ToDouble(amount);
                                        salaryadvance = Math.Round(amount, 0);
                                    }
                                }
                                else
                                {
                                    salaryadvance = 0;
                                    newrow["SALARY ADVANCE"] = salaryadvance;
                                }
                            }
                            // newrow["TOTAL DEDUCTIONS"] = Math.Ceiling(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction);
                            totaldeduction = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + otherdeduction);
                            double netpay = 0;
                            netpay = Math.Ceiling(totalearnings - totaldeduction);
                            netpay = Math.Round(netpay, 2);
                            newrow["NET PAY"] = Math.Ceiling(netpay);
                            Report.Rows.Add(newrow);
                            //string stramount = "0";
                            //stramount = netpay.ToString();
                            //if (stramount == "NaN" || stramount == "0")
                            //{
                            //}
                            //else
                            //{
                            //    newrow["NET PAY"] = Math.Ceiling(netpay);
                            //    Report.Rows.Add(newrow);
                            //}
                        }
                    }
                    else
                    {
                        lblmsg.Text = "No data were found";
                        hidepanel.Visible = false;
                    }

                }
            }
            DataRow newTotal = Report.NewRow();
            newTotal["DESIGNATION"] = "Total Amount";
            double val = 0.0;
            foreach (DataColumn dc in Report.Columns)
            {
                if (dc.DataType == typeof(Double))
                {
                    val = 0.0;
                    double.TryParse(Report.Compute("sum([" + dc.ToString() + "])", "[" + dc.ToString() + "]<>'0'").ToString(), out val);
                    newTotal[dc.ToString()] = val;
                }
            }
            Report.Rows.Add(newTotal);
            if (nonpf.Rows.Count > 0)
            {
                DataRow dtnewTotal = nonpf.NewRow();
                dtnewTotal["DESIGNATION"] = "Total Amount";
                double dtval = 0.0;
                foreach (DataColumn dcc in nonpf.Columns)
                {
                    if (dcc.DataType == typeof(Double))
                    {
                        dtval = 0.0;
                        double.TryParse(nonpf.Compute("sum([" + dcc.ToString() + "])", "[" + dcc.ToString() + "]<>'0'").ToString(), out dtval);
                        dtnewTotal[dcc.ToString()] = dtval;
                    }
                }
                nonpf.Rows.Add(dtnewTotal);



                //DataRow newrow2 = Report.NewRow();
                //newrow2["Employee Code"] = "Non Pf";
                //Report.Rows.Add(newrow2);
                //divnonpf.Visible = true;
                //grdnonpf.DataSource = nonpf;
                //grdnonpf.DataBind();
            }
            else
            {
                divnonpf.Visible = false;
                grdnonpf.DataSource = null;
                grdnonpf.DataBind();
            }
            DataTable dtnew = new DataTable();
            dtnew.Columns.Add("Invoice");
            dtnew.Columns.Add("JV Date");
            dtnew.Columns.Add("Ledger Name");
            dtnew.Columns.Add("Amount");
            dtnew.Columns.Add("Narration");
            string designation = "Total Amount";
            var k = 1;
            string tnarration = "";
            cmd = new SqlCommand("SELECT groupledgername.ledgername, groupledgername.ledgercode, groupledgername.glcodesno, glgroup.glname,glgroup.sno FROM glgroup INNER JOIN groupledgername ON glgroup.sno = groupledgername.glcodesno WHERE (groupledgername.branchid = @BranchID)");
            cmd.Parameters.Add("@BranchID", ddlbranch.SelectedValue);
            DataTable dtledger = vdm.SelectQuery(cmd).Tables[0];

            foreach (DataRow dreport in Report.Select("DESIGNATION='" + designation + "'"))
            {
                double tnetpay = 0;
                double tgross = 0;
                string narration = "";

                DataRow nr_sp = dtnew.NewRow();
                nr_sp["Invoice"] = invoice;
                nr_sp["JV Date"] = talydate;
                string casualpayable = "8";
                string casual_ledger = "";
                foreach (DataRow dr in dtledger.Select("sno='" + casualpayable + "'"))
                {
                    casual_ledger = dr["ledgername"].ToString();
                }
                //nr_sp["Ledger Name"] = casual_ledger;

                nr_sp["Ledger Name"] = "wages payble - " + ddlbranch.SelectedItem.Text + "";
                nr_sp["Amount"] = dreport["GROSS Earnings"].ToString();
                double amt = 0;
                double.TryParse(dreport["NET PAY"].ToString(), out amt);
                amt = Math.Round(amt, 2);
                string amount = "-" + amt + "";
                tnetpay = Convert.ToDouble(amt);
                tgross = Convert.ToDouble(dreport["GROSS Earnings"].ToString());
                narration = "Being The SVDS " + ddlbranch.SelectedItem.Text + " salaries For the Month Of " + ddlmonth.SelectedValue + ", Total Gross=" + tgross + "/-,NetPay=" + tnetpay + "/- ";
                nr_sp["Narration"] = narration;
                dtnew.Rows.Add(nr_sp);
                tnarration = narration;
            }
            if (Report.Rows.Count > 0)
            {
                foreach (DataRow dr in Report.Rows)
                {

                    if (dr["DESIGNATION"] == "Total Amount")
                    {
                    }
                    else
                    {
                        DataRow nr_spp = dtnew.NewRow();
                        nr_spp["Invoice"] = invoice;
                        nr_spp["JV Date"] = talydate;
                        nr_spp["Ledger Name"] = dr["ledgername"].ToString();
                        nr_spp["Amount"] = "-" + dr["GROSS Earnings"].ToString() + "";
                        nr_spp["Narration"] = tnarration;
                        dtnew.Rows.Add(nr_spp);
                    }
                }
            }
            grdReports.DataSource = dtnew;
            grdReports.DataBind();
            Session["xportdata"] = dtnew;
            hidepanel.Visible = true;
        }
        catch (Exception ex)
        {
            lblmsg.Text = ex.Message;
            hidepanel.Visible = false;
        }
    }
}