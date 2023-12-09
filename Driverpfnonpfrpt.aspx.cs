﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

public partial class Driverpfnonpfrpt : System.Web.UI.Page
{
    SqlCommand cmd;
    string userid = "";
    DBManager vdm;

    double netpay = 0;

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
                    lblAddress.Text = Session["Address"].ToString();
                    lblTitle.Text = Session["TitleName"].ToString();
                    bindbranchs();
                    PopulateYear();
                    string frmdate = dtfrom.ToString("dd/MM/yyyy");
                    string[] str = frmdate.Split('/');
                    ddlmonth.SelectedValue = str[1];
                }
            }
        }
    }

    private void PopulateMonth()
    {

        ddlmonth.Items.Clear();
        ListItem lt = new ListItem();
        lt.Text = "MM";
        lt.Value = "00";
        ddlmonth.Items.Add(lt);
        for (int i = 1; i <= 12; i++)
        {
            lt = new ListItem();
            lt.Text = Convert.ToDateTime(i.ToString() + "/1/1900").ToString("MMMM");
            lt.Value = i.ToString();
            ddlmonth.Items.Add(lt);
        }
        ddlmonth.Items.FindByValue(DateTime.Now.Month.ToString()).Selected = true;
    }

    private void PopulateYear()
    {

        ddlyear.Items.Clear();
        ListItem lt = new ListItem();
        lt.Text = "YYYY";
        lt.Value = "0";
        ddlyear.Items.Add(lt);
        for (int i = DateTime.Now.Year; i >= 1970; i--)
        {
            lt = new ListItem();
            lt.Text = i.ToString();
            lt.Value = i.ToString();
            ddlyear.Items.Add(lt);
        }
        ddlyear.Items.FindByValue(DateTime.Now.Year.ToString()).Selected = true;
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
        cmd = new SqlCommand("SELECT branchmaster.branchid, branchmaster.branchname, branchmaster.company_code FROM branchmaster INNER JOIN branchmapping ON branchmaster.branchid = branchmapping.subbranch WHERE (branchmapping.mainbranch = @m)");
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
    DataTable Report = new DataTable();
    protected void btn_Generate_Click(object sender, EventArgs e)
    {
        try
        {
            lblmsg.Text = "";
            DBManager SalesDB = new DBManager();
            DateTime fromdate = DateTime.Now;
            DateTime mydate = DateTime.Now;
            //string year = "2016";
            string year = ddlyear.SelectedItem.Value;
            //string currentyear = (mydate.Year).ToString();
            string mymonth = ddlmonth.SelectedItem.Value;
            int month = Convert.ToInt32(mymonth);
            int years = Convert.ToInt32(year);
            string day = "";
            if (mymonth == "02")
            {
                day = "28";
            }
            else
            {
                day = (mydate.Day).ToString();
                int days = DateTime.DaysInMonth(years, month);
                int cmdays = Convert.ToInt32(day);
                if (cmdays > days)
                {
                    day = days.ToString();
                }
            }
            string date = mymonth + "/" + day + "/" + year;
            DateTime dtfrom = fromdate;
            string frmdate = dtfrom.ToString("dd/MM/yyyy");
            string[] str = frmdate.Split('/');
            lblFromDate.Text = mymonth;
            fromdate = Convert.ToDateTime(date);
            lblHeading.Text = ddlbranch.SelectedItem.Text + " " + ddlemptype.SelectedItem.Text + " Salary Statement For The Month Of " + " " + ddlmonth.SelectedItem.Text + " " + year;
            Session["filename"] = ddlbranch.SelectedItem.Text + " Salary Statement " + " " + ddlmonth.SelectedItem.Text + " " + year;
            Session["title"] = ddlbranch.SelectedItem.Text + " Salary Statement " + " " + ddlmonth.SelectedItem.Text + " " + year;
            cmd = new SqlCommand("SELECT company_master.address, company_master.companyname FROM branchmaster INNER JOIN company_master ON branchmaster.company_code = company_master.sno WHERE (branchmaster.branchid = @BranchID)");
            cmd.Parameters.Add("@BranchID", ddlbranch.SelectedValue);
            DataTable dtcompany = vdm.SelectQuery(cmd).Tables[0];
            if (dtcompany.Rows.Count > 0)
            {
                lblAddress.Text = dtcompany.Rows[0]["address"].ToString();
                lblTitle.Text = dtcompany.Rows[0]["companyname"].ToString();
                Session["TitleName"] = dtcompany.Rows[0]["companyname"].ToString();
                Session["Address"] = dtcompany.Rows[0]["address"].ToString();
            }
            else
            {
                lblAddress.Text = Session["Address"].ToString();
                lblTitle.Text = Session["TitleName"].ToString();
            }

            double totalearnings = 0;
            Report.Columns.Add("Sno");
            Report.Columns.Add("Employee Code");
            Report.Columns.Add("Name");
            Report.Columns.Add("Designation");
            if (ddlbranch.SelectedValue == "6")
            {
                Report.Columns.Add("Gross");
                Report.Columns.Add("Payable Days");
                Report.Columns.Add("Basic").DataType = typeof(double);
                Report.Columns.Add("HRA").DataType = typeof(double);
                Report.Columns.Add("CONVEYANCE ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("MEDICAL ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("WASHING ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("Gross Earnings").DataType = typeof(double);
                Report.Columns.Add("PF").DataType = typeof(double);
            }
            else
            {
                Report.Columns.Add("Rate/Day");
                Report.Columns.Add("Batta/Day");
                Report.Columns.Add("Work Days");
                Report.Columns.Add("Payable Days");
                Report.Columns.Add("Gross Earnings").DataType = typeof(double);
            }
            Report.Columns.Add("PT").DataType = typeof(double);
            Report.Columns.Add("Salary Advance").DataType = typeof(double);
            Report.Columns.Add("Loan").DataType = typeof(double);
            Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
            Report.Columns.Add("MOBILE DEDUCTION").DataType = typeof(double);
            Report.Columns.Add("OTHER DEDUCTIONS").DataType = typeof(double);
            Report.Columns.Add("Total Deductions").DataType = typeof(double);
            Report.Columns.Add("Net Pay").DataType = typeof(double);
            Report.Columns.Add("Bank Acc No");
            Report.Columns.Add("IFSC Code");
            int branchid = Convert.ToInt32(ddlbranch.SelectedItem.Value);
            string pftype = ddlemptype.SelectedItem.Value;
            string employee_type = "Driver";
            string PF = "";
            if (pftype == "pf")
            {
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employedetails.pfdate, employedetails.esidate, employebankdetails.ifsccode, employedetails.esieligible, monthly_attendance.month,monthly_attendance.year, employedetails.employee_type, employedetails.empid, employedetails.employee_num, employedetails.fullname,designation.designation, employebankdetails.accountno, branchmaster.statename, employedetails.employee_dept, departments.department,salaryappraisals.gross, salaryappraisals.erningbasic, salaryappraisals.hra, salaryappraisals.profitionaltax, salaryappraisals.esi, salaryappraisals.providentfund,salaryappraisals.conveyance, salaryappraisals.medicalerning, salaryappraisals.washingallowance, salaryappraisals.travelconveyance, salaryappraisals.salaryperyear FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN departments ON employedetails.employee_dept = departments.deptid INNER JOIN salaryappraisals ON employedetails.empid = salaryappraisals.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.status = 'No') AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.employee_type = @emptype) AND (employedetails.pfeligible=@PF) AND (employedetails.branchid = @branchid) AND (salaryappraisals.endingdate IS NULL) AND (salaryappraisals.startingdate <= @d1) OR (employedetails.status = 'No') AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.employee_type = @emptype) AND (employedetails.pfeligible=@PF) AND (employedetails.branchid = @branchid) AND (salaryappraisals.endingdate > @d1) AND (salaryappraisals.startingdate <= @d1)");
                PF = "Yes";
            }
            else
            {
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employedetails.pfdate, employedetails.esidate, employebankdetails.ifsccode, employedetails.esieligible, monthly_attendance.month,monthly_attendance.year, employedetails.employee_type, employedetails.empid, employedetails.employee_num, employedetails.fullname,designation.designation, employebankdetails.accountno, branchmaster.statename, employedetails.employee_dept, departments.department,salaryappraisals.gross, salaryappraisals.erningbasic, salaryappraisals.hra, salaryappraisals.profitionaltax, salaryappraisals.esi, salaryappraisals.providentfund,salaryappraisals.conveyance, salaryappraisals.medicalerning, salaryappraisals.washingallowance, salaryappraisals.travelconveyance, salaryappraisals.salaryperyear FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN departments ON employedetails.employee_dept = departments.deptid INNER JOIN salaryappraisals ON employedetails.empid = salaryappraisals.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.status = 'No') AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.employee_type = @emptype) AND (employedetails.pfeligible=@PF) AND (employedetails.branchid = @branchid) AND (salaryappraisals.endingdate IS NULL) AND (salaryappraisals.startingdate <= @d1) OR (employedetails.status = 'No') AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.employee_type = @emptype) AND (employedetails.pfeligible=@PF) AND (employedetails.branchid = @branchid) AND (salaryappraisals.endingdate > @d1) AND (salaryappraisals.startingdate <= @d1)");
                PF = "No";
            }
            cmd.Parameters.Add("@branchid", branchid);
            cmd.Parameters.Add("@PF", PF);
            cmd.Parameters.Add("@month", mymonth);
            cmd.Parameters.Add("@year", year);
            cmd.Parameters.Add("@emptype", employee_type);
            cmd.Parameters.Add("@d1", date);
            DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
            cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid,monthly_attendance.clorwo, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN  employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid WHERE  (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
            cmd.Parameters.Add("@month", mymonth);
            cmd.Parameters.Add("@year", year);
            cmd.Parameters.Add("@branchid", branchid);
            DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
            cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) AND (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
            cmd.Parameters.Add("@month", mymonth);
            cmd.Parameters.Add("@year", year);
            cmd.Parameters.Add("@branchid", branchid);
            DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
            cmd = new SqlCommand("SELECT employedetails.employee_num, loan_request.loanemimonth, loan_request.month FROM employedetails INNER JOIN  loan_request ON employedetails.empid = loan_request.empid where (employedetails.branchid=@branchid) AND (loan_request.month = @month) AND (loan_request.year = @year)");
            cmd.Parameters.Add("@month", mymonth);
            cmd.Parameters.Add("@year", year);
            cmd.Parameters.Add("@branchid", branchid);
            DataTable dtloan = vdm.SelectQuery(cmd).Tables[0];
            cmd = new SqlCommand("SELECT  employedetails.employee_num, mobile_deduction.deductionamount, employedetails.fullname, employedetails.empid FROM  mobile_deduction INNER JOIN employedetails ON mobile_deduction.employee_num = employedetails.employee_num   where (employedetails.branchid=@branchid) AND (mobile_deduction.month = @month) AND (mobile_deduction.year = @year)");
            cmd.Parameters.Add("@branchid", branchid);
            cmd.Parameters.Add("@month", mymonth);
            cmd.Parameters.Add("@year", year);
            DataTable dtmobile = vdm.SelectQuery(cmd).Tables[0];
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
                int days = 0;
                if (mymonth == "02")
                {
                    days = 31;
                }
                else
                {
                    if (mymonth == "03")
                    {
                        days = 28;
                    }
                    else
                    {
                        string prevmnth = "";
                        string pyear = "";
                        if (mymonth == "01")
                        {
                            prevmnth = "12";
                            pyear = (Convert.ToInt32(year) - 1).ToString();
                        }
                        else
                        {
                            prevmnth = (Convert.ToInt32(mymonth) - 1).ToString();
                            pyear = year;
                        }
                        string prevdate = "24";
                        string curdate = "24";
                        prevdate = prevmnth + "/" + prevdate + "/" + pyear;
                        curdate = mymonth + "/" + curdate + "/" + year;
                        DateTime dtcureentdate = Convert.ToDateTime(curdate);
                        DateTime prevdatetime = Convert.ToDateTime(prevdate);
                        TimeSpan difference = dtcureentdate - prevdatetime;
                        string totaldays = difference.TotalDays.ToString();
                        days = Convert.ToInt32(totaldays);
                       // days = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(mymonth));
                    }
                }
                foreach (DataRow dr in dtsalary.Rows)
                {
                    double totalpresentdays = 0;
                    double profitionaltax = 0;
                    double salaryadvance = 0;
                    double loan = 0;
                    double canteendeduction = 0;
                    double mobilededuction = 0;
                    double totaldeduction;
                    double numberofworkingdays = 0;
                    double providentfund = 0;
                    double daysinmonth = 0;
                    string statename = "";
                    DataRow newrow = Report.NewRow();
                    newrow["Sno"] = i++.ToString();
                    //newrow["Employeeid"] = dr["empid"].ToString();
                    newrow["Employee Code"] = dr["employee_num"].ToString();
                    newrow["Name"] = dr["fullname"].ToString();
                    newrow["Designation"] = (dr["department"].ToString() + " " + dr["designation"].ToString());
                    //newrow["Department"] = dr["department"].ToString();
                    double Basic = Convert.ToDouble(dr["erningbasic"].ToString());
                    statename = dr["statename"].ToString();
                    //newrow["PT"] = dr["profitionaltax"].ToString();
                    double rateper = Convert.ToDouble(dr["gross"].ToString());
                    //profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
                    newrow["Bank Acc No"] = dr["accountno"].ToString();
                    newrow["IFSC Code"] = dr["ifsccode"].ToString();
                    //Report.Rows.Add(newrow);
                    if (dtattendence.Rows.Count > 0)
                    {
                        foreach (DataRow dra in dtattendence.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                        {

                            double.TryParse(dra["numberofworkingdays"].ToString(), out numberofworkingdays);
                            double clorwo = 0;
                            double.TryParse(dra["clorwo"].ToString(), out clorwo);
                            daysinmonth = numberofworkingdays + clorwo;

                            //double rateperday = 0;
                            double paydays = 0;
                            double lop = 0;
                            double.TryParse(dra["lop"].ToString(), out lop);
                            paydays = daysinmonth - lop;
                            double holidays = 0;
                            holidays = daysinmonth - numberofworkingdays;
                            newrow["Payable Days"] = paydays;
                            
                            totalpresentdays = holidays + paydays;
                            //double perdaybasic = Basic / daysinmonth;
                            double rate = Convert.ToDouble(dr["gross"].ToString());

                            double batta = 0;


                            if (ddlbranch.SelectedValue == "6")
                            {
                                batta = 0;
                                double GROSSSAL = rateper * 15;
                                if (days == 31)
                                {
                                    if (lop > 16)
                                    {
                                        lop = lop - 15;
                                    }
                                    else
                                    {
                                        lop = 0;
                                    }
                                }
                                else
                                {
                                    if (days == 28)
                                    {
                                        if (lop > 13)
                                        {
                                            lop = lop - 13;
                                        }
                                        else
                                        {
                                            lop = 0;
                                        }
                                    }
                                    else
                                    {
                                        if (lop > 15)
                                        {
                                            lop = lop - 15;
                                        }
                                        else
                                        {
                                            lop = 0;
                                        }
                                    }
                                    
                                }
                                double loss = lop * rateper;
                                double gainsal = GROSSSAL - loss;
                                double BasicSAL = 50;
                                BasicSAL = (gainsal * 50) / 100;
                                double CONALLAWANCE = 1600;
                                double perdayconveyance = CONALLAWANCE / 15;
                                double loseofconviyance = lop * perdayconveyance;
                                double MEDICALALLAWANCE = 1250;
                                double perdayMEDICALALLAWANCE = MEDICALALLAWANCE / 15;
                                double loseofMEDICALALLAWANCE = lop * perdayMEDICALALLAWANCE;
                                double WASHINGALLAWANCE = 1000;
                                double perdayWASHINGALLAWANCE = WASHINGALLAWANCE / 15;
                                double loseofWASHINGALLAWANCE = lop * perdayWASHINGALLAWANCE;
                                CONALLAWANCE = Math.Round(CONALLAWANCE - loseofconviyance, 0);
                                MEDICALALLAWANCE = Math.Round(MEDICALALLAWANCE - loseofMEDICALALLAWANCE, 0);
                                WASHINGALLAWANCE = Math.Round(WASHINGALLAWANCE - loseofWASHINGALLAWANCE, 0);
                                double totalerngs = BasicSAL + CONALLAWANCE + MEDICALALLAWANCE + WASHINGALLAWANCE;
                                double thra = gainsal;
                                double hra = Math.Round(thra - totalerngs);
                                double HRA = hra;
                                newrow["Gross"] = GROSSSAL;
                                newrow["Basic"] = BasicSAL;
                                newrow["HRA"] = HRA;
                                newrow["CONVEYANCE ALLOWANCE"] = CONALLAWANCE;
                                newrow["MEDICAL ALLOWANCE"] = MEDICALALLAWANCE;
                                newrow["WASHING ALLOWANCE"] = WASHINGALLAWANCE;
                            }
                            else
                            {
                                batta = 80;
                                newrow["Work Days"] = daysinmonth.ToString();
                                newrow["Rate/Day"] = rateper;
                                string empnumber = dr["employee_num"].ToString();
                                if (empnumber == "SVDS080135" || empnumber == "SVDS100095")
                                {
                                    newrow["Batta/Day"] = "0";
                                    batta = 0;
                                }
                                else
                                {
                                    newrow["Batta/Day"] = batta;
                                }
                            }
                            double rateperday = rate + batta;
                            totalearnings = rateperday * paydays;
                            double totalpdays = numberofworkingdays - lop;
                            totalearnings = Math.Round(totalearnings);
                            newrow["Gross Earnings"] = totalearnings;
                            string pfeligible = dr["pfeligible"].ToString();
                            if (pfeligible == "Yes")
                            {
                                providentfund = (totalearnings * 6) / 100;
                                if (providentfund > 1800)
                                {
                                    providentfund = 1800;
                                }
                                providentfund = Math.Round(providentfund, 0);
                                newrow["PF"] = Math.Round(providentfund, 0);
                            }
                            else
                            {
                                providentfund = 0;
                                newrow["PF"] = providentfund;
                            }
                            if (statename == "AndraPrdesh")
                            {
                                if (totalearnings > 1000 && totalearnings <= 15000)
                                {
                                    profitionaltax = 0;
                                }
                                else if (totalearnings >= 15001 && totalearnings <= 20000)
                                {
                                    profitionaltax = 150;
                                }
                                else if (totalearnings >= 20001)
                                {
                                    profitionaltax = 200;
                                }
                            }
                            if (statename == "Tamilnadu")
                            {
                                if (totalearnings < 7000)
                                {
                                    profitionaltax = 0;
                                }
                                else if (totalearnings >= 7001 && totalearnings <= 10000)
                                {
                                    profitionaltax = 115;
                                }
                                else if (totalearnings >= 10001 && totalearnings <= 11000)
                                {
                                    profitionaltax = 171;
                                }
                                else if (totalearnings >= 11001 && totalearnings <= 12000)
                                {
                                    profitionaltax = 171;
                                }
                                else if (totalearnings >= 12001)
                                {
                                    profitionaltax = 208;
                                }
                            }
                            if (statename == "karnataka")
                            {
                                if (totalearnings <= 15000 && totalearnings <= 15001)
                                {
                                    profitionaltax = 0;
                                }
                                else if (totalearnings >= 15001)
                                {
                                    profitionaltax = 200;
                                }
                            }
                            newrow["PT"] = profitionaltax;
                        }
                    }



                    if (dtsa.Rows.Count > 0)
                    {
                        DataRow[] drr = dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                        if (drr.Length > 0)
                        {
                            foreach (DataRow drsa in dtsa.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                            {
                                double amount = 0;
                                double.TryParse(drsa["amount"].ToString(), out amount);
                                newrow["Salary Advance"] = amount.ToString();
                                salaryadvance = Convert.ToDouble(amount);
                                salaryadvance = Math.Round(amount, 0);
                            }
                        }
                        else
                        {
                            salaryadvance = 0;
                            newrow["Salary Advance"] = salaryadvance;
                        }
                    }
                    if (dtloan.Rows.Count > 0)
                    {
                        DataRow[] drr = dtloan.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                        if (drr.Length > 0)
                        {
                            foreach (DataRow drloan in dtloan.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                            {
                                double loanemimonth = 0;
                                double.TryParse(drloan["loanemimonth"].ToString(), out loanemimonth);
                                newrow["Loan"] = loanemimonth.ToString();
                                loan = Convert.ToDouble(loanemimonth);
                                loan = Math.Round(loanemimonth, 0);
                            }
                        }
                        else
                        {
                            loan = 0;
                            newrow["Loan"] = loan;
                        }
                    }
                    mobilededuction = 0;
                    if (dtmobile.Rows.Count > 0)
                    {
                        DataRow[] drr = dtmobile.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                        if (drr.Length > 0)
                        {
                            foreach (DataRow drmobile in dtmobile.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                            {
                                double deductionamount = 0;
                                double.TryParse(drmobile["deductionamount"].ToString(), out deductionamount);
                                newrow["MOBILE DEDUCTION"] = deductionamount.ToString();
                                string st = deductionamount.ToString();
                                if (st == "0.0")
                                {
                                    mobilededuction = 0;
                                    newrow["MOBILE DEDUCTION"] = mobilededuction;

                                }
                                else
                                {
                                    mobilededuction = Convert.ToDouble(deductionamount);
                                    mobilededuction = Math.Round(deductionamount, 0);
                                }
                            }
                        }
                        else
                        {
                            mobilededuction = 0;
                            newrow["MOBILE DEDUCTION"] = mobilededuction;
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
                                canteendeduction = Math.Round(deductionamount, 0);
                            }
                        }
                        else
                        {
                            canteendeduction = 0;
                            newrow["CANTEEN DEDUCTION"] = canteendeduction;
                        }
                    }
                    double otherdeduction = 0;
                    if (dtotherdeduction.Rows.Count > 0)
                    {
                        DataRow[] drr = dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                        if (drr.Length > 0)
                        {
                            foreach (DataRow drotherdeduction in dtotherdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                            {
                                double amount = 0;
                                double.TryParse(drotherdeduction["otherdeductionamount"].ToString(), out amount);
                                newrow["OTHER DEDUCTIONS"] = amount.ToString();
                                string st = amount.ToString();
                                otherdeduction = Convert.ToDouble(amount);
                                otherdeduction = Math.Round(amount, 0);
                            }
                        }
                        else
                        {
                            otherdeduction = 0;
                            newrow["OTHER DEDUCTIONS"] = otherdeduction;
                        }
                    }
                    else
                    {
                    }
                    newrow["Total Deductions"] = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + otherdeduction + providentfund);
                    totaldeduction = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + otherdeduction + providentfund);
                    double netpay = 0;
                    netpay = Math.Round(totalearnings - totaldeduction);
                    netpay = Math.Round(netpay, 0);
                    string stramount = "";
                    stramount = netpay.ToString();
                    if (stramount == "NaN" || stramount == "" || numberofworkingdays == 0)
                    {
                    }
                    else
                    {
                        newrow["Net Pay"] = Math.Round(netpay, 0);
                        Report.Rows.Add(newrow);
                    }
                }
            }
            if (Report.Rows.Count > 0)
            {
                DataRow newTotal = Report.NewRow();
                newTotal["Designation"] = "Total Amount";
                double val = 0.0;
                foreach (DataColumn dc in Report.Columns)
                {
                    if (dc.DataType == typeof(Double))
                    {
                        val = 0.0;
                        double.TryParse(Report.Compute("sum([" + dc.ToString() + "])", "[" + dc.ToString() + "]<>'0'").ToString(), out val);
                        if (val == 0.0)
                        {
                        }
                        else
                        {
                            newTotal[dc.ToString()] = val;
                        }
                    }
                }

                Report.Rows.Add(newTotal);
                foreach (var column in Report.Columns.Cast<DataColumn>().ToArray())
                {
                    if (Report.AsEnumerable().All(dr => dr.IsNull(column)))
                        Report.Columns.Remove(column);

                }

                grdReports.DataSource = Report;
                grdReports.DataBind();
                Session["xportdata"] = Report;
                hidepanel.Visible = true;
            }
            else
            {
                hidepanel.Visible = false;
                lblmsg.Text = "No Data Found";
            }
        }

        catch (Exception ex)
        {
            lblmsg.Text = ex.Message;
            hidepanel.Visible = false;
        }
    }

    protected void grdReports_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            //e.Row.Cells[4].Visible = false;
            if (e.Row.Cells.Count > 2)
            {
                if (e.Row.Cells[3].Text == "Total Amount")
                {
                    e.Row.BackColor = System.Drawing.Color.Aquamarine;
                    e.Row.Font.Size = FontUnit.Large;
                    e.Row.Font.Bold = true;

                }
            }
        }
    }

    protected void grdReports_DataBinding(object sender, EventArgs e)
    {
        try
        {
            string mainb = Session["mainbranch"].ToString();
            if (mainb == "6")
            {
                if (ddlemptype.SelectedItem.Text == "Staff")
                {
                    GridViewGroup First = new GridViewGroup(grdReports, null, "Department");
                    // GridViewGroup three = new GridViewGroup(grdReports, seconf, "PF");
                }
            }
            else
            {

            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void btnlogssave_click(object sender, EventArgs e)
    {
        DBManager SalesDB = new DBManager();
        DateTime ServerDateCurrentdate = DBManager.GetTime(vdm.conn);
        DataTable empid = Session["empid"] as DataTable;
        string mainbranch = Session["mainbranch"].ToString();
        cmd = new SqlCommand("SELECT employedetails.empid, employedetails.employee_num, employedetails.fullname, departments.department, departments.deptid, designation.designation, designation.designationid FROM employedetails INNER JOIN branchmapping ON employedetails.branchid = branchmapping.subbranch INNER JOIN departments ON employedetails.employee_dept = departments.deptid INNER JOIN designation ON employedetails.designationid = designation.designationid WHERE(branchmapping.mainbranch = @m) AND (employedetails.status = 'No')");
        cmd.Parameters.Add("@m", mainbranch);
        DataTable dtemployee = vdm.SelectQuery(cmd).Tables[0];
        //DataTable designationid = Session["designationid"] as DataTable;
        //cmd = new SqlCommand("SELECT designationid, designation FROM designation ");
        //DataTable dtdesignation = vdm.SelectQuery(cmd).Tables[0];
        DataTable dtlogs = new DataTable();
        dtlogs = (DataTable)Session["xportdata"];
        if (dtlogs.Rows.Count > 0)
        {
            DateTime doe = DateTime.Now;
            string year = ddlyear.SelectedItem.Value;
            string branchid = ddlbranch.SelectedItem.Value;
            string month = ddlmonth.SelectedItem.Value;
            string type = ddlemptype.SelectedItem.Value;
            int premonth = 0;
            string newmonth = "0";
            int.TryParse(month, out premonth);
            premonth = premonth - 1;
            if (premonth >= 10)
            {
                newmonth = premonth.ToString();
            }
            else
            {
                newmonth = "0" + premonth;
            }
            string emptype = "Driver";
            cmd = new SqlCommand("SELECT COUNT(month) AS Month FROM  monthlysalarystatement WHERE (month = @month) AND (emptype = @emptype) AND (year = @year) AND (branchid = @branch) HAVING (COUNT(month) > 0)");
            cmd.Parameters.Add("@branch", branchid);
            cmd.Parameters.Add("@year", year);
            cmd.Parameters.Add("@emptype", emptype);
            cmd.Parameters.Add("@month", premonth);
            DataTable dtmonth = vdm.SelectQuery(cmd).Tables[0];
            if (dtmonth.Rows.Count > 0)
            {
                cmd = new SqlCommand("SELECT * FROM monthlysalarystatement WHERE   branchid=@branch AND month=@month and year=@year and emptype=@emptype and type=@type");
                cmd.Parameters.Add("@branch", branchid);
                cmd.Parameters.Add("@month", month);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@emptype", emptype);
                cmd.Parameters.Add("@type", type);
                DataTable dtdata = vdm.SelectQuery(cmd).Tables[0];
                if (dtdata.Rows.Count > 0)
                {
                    lblmsg.Text = "THIS MONTH OF DATA ALREADY CLOSED ";
                    return;
                }
                else
                {

                    foreach (DataRow dr in dtlogs.Rows)
                    {
                        string employeeid = "";
                        string employeid = dr["Employee Code"].ToString();
                        foreach (DataRow dremployee in dtemployee.Select("employee_num='" + employeid + "'"))
                        {
                            employeeid = dremployee["empid"].ToString();
                        }

                        string desigationid = "";

                        string designation = dr["Employee Code"].ToString();
                        foreach (DataRow dremployee in dtemployee.Select("employee_num='" + designation + "'"))
                        {
                            desigationid = dremployee["designationid"].ToString();
                        }


                        //string design = (dr["department"].ToString() + "-" + dr["designation"].ToString());
                        //string[] tokens = design.Split('-');
                        //string desig = tokens[0];
                        //string dept = tokens[1]; 
                        string deptid = "";
                        string Department = dr["Employee Code"].ToString();
                        foreach (DataRow dremployee in dtemployee.Select("employee_num='" + Department + "'"))
                        {
                            deptid = dremployee["deptid"].ToString();
                        }

                        string desgn = dr["Designation"].ToString();
                        if (desgn == "Total Amount")
                        {

                        }
                        else
                        {
                            desgn = dr["Designation"].ToString();
                            //string emptype = ddlemptype.SelectedItem.Value;
                            string empcode = dr["Employee Code"].ToString();
                            string Name = dr["Name"].ToString();
                            string daysmonth = "0";
                            try
                            {
                                daysmonth = dr["Days Month"].ToString();
                            }
                            catch
                            {
                            }
                            try
                            {
                                daysmonth = dr["Total Days in Month"].ToString();
                            }
                            catch
                            {
                            }
                            string WorkDays = "0";
                            try
                            {
                                WorkDays = dr["Work Days"].ToString();
                            }
                            catch
                            {
                            }
                            string BetaperDay = "0";
                            try
                            {
                                BetaperDay = dr["Batta/Day"].ToString();
                            }
                            catch
                            {
                            }
                            string attandancedays = "0";
                            try
                            {
                                attandancedays = dr["Attendance Days"].ToString();
                            }
                            catch
                            {
                            }

                            string clandholidayoff = "0";
                            try
                            {
                                clandholidayoff = dr["CL Holiday And Off"].ToString();
                            }
                            catch
                            {
                            }

                            string payabledays = "0";
                            try
                            {
                                payabledays = dr["Payable Days"].ToString();
                            }
                            catch
                            {
                            }
                            string Attadencebonusdays = "0";
                            try
                            {
                                Attadencebonusdays = dr["AttendanceBonus>=26days@rs.20"].ToString();
                            }
                            catch
                            {
                            }


                            string salary = "0";
                            try
                            {
                                salary = dr["Gross"].ToString();
                            }
                            catch
                            {
                            }
                            try
                            {
                                salary = dr[" Rate/Day	"].ToString();
                            }
                            catch
                            {
                            }
                            string basic = "0";
                            try
                            {
                                basic = dr["Basic"].ToString();
                            }
                            catch
                            {
                            }

                            string hra = "0";
                            try
                            {
                                hra = dr["HRA"].ToString();
                            }
                            catch
                            {
                            }
                            string conveyance = "0";
                            try
                            {
                                conveyance = dr["Conveyance Allowance"].ToString();
                            }
                            catch
                            {
                            }
                            string medical = "0";
                            try
                            {
                                medical = dr["Medical Allowance"].ToString();
                            }
                            catch
                            {
                            }
                            string washing = "0";
                            try
                            {
                                washing = dr["Washing Allowance"].ToString();
                            }
                            catch
                            {
                            }
                            string gross = dr["Gross Earnings"].ToString();
                            string pt = "0";
                            try
                            {
                                pt = dr["PT"].ToString();
                            }
                            catch
                            {
                            }
                            string pf = "0";
                            try
                            {
                                pf = dr["PF"].ToString();
                            }
                            catch
                            {
                            }
                            string esi = "0";
                            try
                            {
                                esi = dr["ESI"].ToString(); ;
                            }
                            catch
                            {
                            }

                            string salaryadvance = "0";
                            try
                            {
                                salaryadvance = dr["Salary Advance"].ToString();
                            }
                            catch
                            {
                            }
                            string loan = "0";
                            try
                            {
                                loan = dr["Loan"].ToString();
                            }
                            catch
                            {
                            }

                            string canteendeduction = "0";
                            try
                            {
                                canteendeduction = dr["Canteen Deduction"].ToString();
                            }
                            catch
                            {
                            }
                            string mobilededuction = "0";
                            try
                            {
                                mobilededuction = dr["Mobile Deduction"].ToString();
                            }
                            catch
                            {
                            }
                            string mediclim = "0";
                            try
                            {
                                mediclim = dr["Mediclaim Deduction"].ToString();
                            }
                            catch
                            {
                            }
                            string otherdeduction = "0";
                            try
                            {
                                otherdeduction = dr["Other Deductions"].ToString();
                            }
                            catch
                            {
                            }
                            string tdsdeduction = "0";
                            try
                            {
                                tdsdeduction = dr["Tds Deduction"].ToString();
                            }
                            catch
                            {
                            }
                            string sempid = Session["empid"].ToString();
                            string totaldeduction = dr["Total Deductions"].ToString();
                            string netpay = dr["Net Pay"].ToString();
                            string bankaccountno = dr["Bank Acc No"].ToString();
                            string ifsccode = dr["IFSC Code"].ToString();
                            //if (empcode != )
                            //{
                            cmd = new SqlCommand("insert into monthlysalarystatement(employecode, empname, designation, daysmonth, attandancedays, clandholidayoff, payabledays, basic, hra, conveyance, medical, washing, gross, pt, pf, esi,  salaryadvance, loan, canteendeduction, mobilededuction, mediclim, otherdeduction, totaldeduction, netpay, bankaccountno, ifsccode, emptype, branchid, month, dateofclosing, closedby,Tdsdeduction,betaperday,deptid,year,attendancebonus,empid,salary,entry_by,entry_date,type) Values (@employecode, @empname, @designation, @daysmonth, @attandancedays, @clandholidayoff, @payabledays, @basic, @hra, @conveyance, @medical, @washing,@gross,@pt,@pf,@esi,@salaryadvance,@loan,@canteendeduction,@mobilededuction,@mediclim,@otherdeduction,@totaldeduction,@netpay,@bankaccountno,@ifsccode,@iemptype,@branchid,@month,@dateofclosing,@closedby,@Tdsdeduction,@betaperday,@deptid,@year,@attendancebonus,@empid,@salary,@entry_by,@entry_date,@itype)");
                            cmd.Parameters.Add("@employecode", empcode);
                            cmd.Parameters.Add("@empname", Name);
                            cmd.Parameters.Add("@designation", desigationid);
                            cmd.Parameters.Add("@daysmonth", daysmonth);
                            cmd.Parameters.Add("@attandancedays", attandancedays);
                            cmd.Parameters.Add("@clandholidayoff", clandholidayoff);
                            cmd.Parameters.Add("@payabledays", payabledays);
                            cmd.Parameters.Add("@salary", salary);
                            cmd.Parameters.Add("@basic", basic);
                            cmd.Parameters.Add("@hra", hra);
                            cmd.Parameters.Add("@conveyance", conveyance);
                            cmd.Parameters.Add("@medical", medical);
                            cmd.Parameters.Add("@washing", washing);
                            cmd.Parameters.Add("@gross", gross);
                            cmd.Parameters.Add("@pt", pt);
                            cmd.Parameters.Add("@pf", pf);
                            cmd.Parameters.Add("@esi", esi);
                            cmd.Parameters.Add("@attendancebonus", Attadencebonusdays);
                            cmd.Parameters.Add("@salaryadvance", salaryadvance);
                            cmd.Parameters.Add("@loan", loan);
                            cmd.Parameters.Add("@canteendeduction", canteendeduction);
                            cmd.Parameters.Add("@mobilededuction", mobilededuction);
                            cmd.Parameters.Add("@mediclim", mediclim);
                            cmd.Parameters.Add("@otherdeduction", otherdeduction);
                            cmd.Parameters.Add("@totaldeduction", totaldeduction);
                            cmd.Parameters.Add("@Tdsdeduction", tdsdeduction);
                            cmd.Parameters.Add("@netpay", netpay);
                            cmd.Parameters.Add("@bankaccountno", bankaccountno);
                            cmd.Parameters.Add("@ifsccode", ifsccode);
                            cmd.Parameters.Add("@iemptype", emptype);
                            cmd.Parameters.Add("@branchid", branchid);
                            cmd.Parameters.Add("@month", month);
                            cmd.Parameters.Add("@dateofclosing", doe);
                            cmd.Parameters.Add("@closedby", Session["empid"]);
                            cmd.Parameters.Add("@betaperday", BetaperDay);
                            cmd.Parameters.Add("@deptid", deptid);
                            cmd.Parameters.Add("@year", year);
                            cmd.Parameters.Add("@empid", employeeid);
                            cmd.Parameters.Add("@entry_by", sempid);
                            cmd.Parameters.Add("@entry_date", ServerDateCurrentdate);
                            cmd.Parameters.Add("@itype", type);
                            SalesDB.insert(cmd);
                            lblmsg.Text = " Employee are successfully saved";                   //}
                        }
                    }
                }
            }
            else
            {
                lblmsg.Text = "Please finalize previous month salary";
            }
        }

    }  
}