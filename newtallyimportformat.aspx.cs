﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

public partial class newtallyimportformat : System.Web.UI.Page
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
                    FillBranches();
                    bindemployeetype();
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

    void FillBranches()
    {
        DBManager SalesDB = new DBManager();
        //branchwise
        //cmd = new SqlCommand("SELECT branchid, branchname,company_code FROM branchmaster where branchmaster.company_code='1'");
        string mainbranch = Session["mainbranch"].ToString();
        //branch mapping
        cmd = new SqlCommand("SELECT branchmaster.branchid, branchmaster.branchname, branchmaster.company_code FROM branchmaster INNER JOIN branchmapping ON branchmaster.branchid = branchmapping.subbranch WHERE (branchmapping.mainbranch = @m) ");
        cmd.Parameters.Add("@m", mainbranch);
        DataTable dttrips = vdm.SelectQuery(cmd).Tables[0];
        ddlbranches.DataSource = dttrips;
        ddlbranches.DataTextField = "branchname";
        ddlbranches.DataValueField = "branchid";
        ddlbranches.DataBind();
    }

    private void bindemployeetype()
    {
        DBManager SalesDB = new DBManager();
        cmd = new SqlCommand("SELECT employee_type FROM employedetails where (employee_type<>'')   GROUP BY employee_type");
        DataTable dttrips = vdm.SelectQuery(cmd).Tables[0];
        ddlemptype.DataSource = dttrips;
        ddlemptype.DataTextField = "employee_type";
        ddlemptype.DataValueField = "employee_type";
        ddlemptype.DataBind();
        ddlemptype.ClearSelection();
        ddlemptype.Items.Insert(0, new ListItem { Value = "0", Text = "--Select Type--", Selected = true });
        ddlemptype.SelectedValue = "0";
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
    DataTable Report = new DataTable();
    protected void btn_Generate_Click(object sender, EventArgs e)
    {

        try
        {
            lblmsg.Text = "";
            DBManager SalesDB = new DBManager();
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
            string d = "00";
            string date = mymonth + "/" + day + "/" + year;
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
            string namonth = dt.ToString("MMM yy");
            Session["xporttype"] = "TallySalarys";

            lblHeading.Text = ddlbranches.SelectedItem.Text + " Tally Salary Statement" + ddlmonth.SelectedItem.Text + year;
            DateTime dtfrom = fromdate;
            string frmdate = dtfrom.ToString("dd/MM/yyyy");
            string[] str = frmdate.Split('/');
            lblFromDate.Text = mymonth;

            fromdate = Convert.ToDateTime(date);
            Session["filename"] = ddlbranches.SelectedItem.Text + " SAP Salary Statement " + ddlmonth.SelectedItem.Text + year;
            Session["title"] = ddlbranches.SelectedItem.Text + " SAP Salary Statement " + ddlmonth.SelectedItem.Text + year;
            string invoivebranch = "";
            cmd = new SqlCommand("SELECT company_master.address, company_master.companyname, branchmaster.branchcode FROM branchmaster INNER JOIN company_master ON branchmaster.company_code = company_master.sno WHERE (branchmaster.branchid = @BranchID)");
            cmd.Parameters.Add("@BranchID", ddlbranches.SelectedValue);
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

            string invoice = "SAL_" + invoivebranch + "JV" + mymonth + "";

            if (ddlemptype.SelectedItem.Text == "Staff" || ddlemptype.SelectedItem.Text == "Permanent")
            {

                Report.Columns.Add("SNO");
                Report.Columns.Add("Employee Code");
                Report.Columns.Add("Name");
                Report.Columns.Add("ledgername");
                Report.Columns.Add("ledgercode");
                Report.Columns.Add("DESIGNATION");
                Report.Columns.Add("GROSS").DataType = typeof(double);
                Report.Columns.Add("DAYS MONTH").DataType = typeof(double);
                Report.Columns.Add("Attendance Days").DataType = typeof(double);
                Report.Columns.Add("CL HOLIDAY AND OFF").DataType = typeof(double);
                Report.Columns.Add("Payable Days").DataType = typeof(double);
                Report.Columns.Add("BASIC").DataType = typeof(double);
                Report.Columns.Add("HRA").DataType = typeof(double);
                Report.Columns.Add("CONVEYANCE ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("MEDICAL ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("WASHING ALLOWANCE").DataType = typeof(double);
                Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                Report.Columns.Add("PT").DataType = typeof(double);
                Report.Columns.Add("PF").DataType = typeof(double);
                Report.Columns.Add("ESI").DataType = typeof(double);
                Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                Report.Columns.Add("Loan").DataType = typeof(double);
                Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("MOBILE DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("MEDICLAIM DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("Tds DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("TOTAL DEDUCTIONS").DataType = typeof(double);
                Report.Columns.Add("NET PAY").DataType = typeof(double);
                Report.Columns.Add("Bank Acc NO");
                Report.Columns.Add("IFSC Code");
                int branchid = Convert.ToInt32(ddlbranches.SelectedItem.Value);
                string employee_type = ddlemptype.SelectedItem.Value;
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employedetails.pfdate, employedetails.ledgercode, employedetails.ledgername,employedetails.esidate, employedetails.esieligible, employedetails.employee_type, employedetails.empid, employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.salaryperyear, pay_structure.hra, pay_structure.conveyance, pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno, employebankdetails.ifsccode, monthly_attendance.month, monthly_attendance.year FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN  monthly_attendance ON employedetails.empid = monthly_attendance.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@emptype", employee_type);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
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
                cmd = new SqlCommand("SELECT employedetails.employee_num, loan_request.loanemimonth, loan_request.months, loan_request.startdate FROM employedetails INNER JOIN  loan_request ON employedetails.empid = loan_request.empid where (employedetails.branchid=@branchid) AND (loan_request.month = @month) AND (loan_request.year = @year)");
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
                cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, mediclaimdeduction.medicliamamount FROM employedetails INNER JOIN mediclaimdeduction ON employedetails.empid = mediclaimdeduction.empid WHERE (employedetails.branchid = @branchid) AND (mediclaimdeduction.flag='1')");
                // cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, mediclaimdeduction.medicliamamount FROM employedetails INNER JOIN mediclaimdeduction ON employedetails.empid = mediclaimdeduction.empid WHERE (employedetails.branchid = @branchid)");
                cmd.Parameters.Add("@branchid", branchid);
                //cmd.Parameters.Add("@month", mymonth);
                //cmd.Parameters.Add("@year", str[2]);
                DataTable dtmedicliam = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, otherdeduction.otherdeductionamount FROM employedetails INNER JOIN otherdeduction ON employedetails.empid = otherdeduction.empid WHERE (employedetails.branchid = @branchid) AND (otherdeduction.month = @month) AND (otherdeduction.year = @year)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                DataTable dtotherdeduction = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, tds_deduction.tdsamount FROM employedetails INNER JOIN tds_deduction ON employedetails.empid = tds_deduction.empid WHERE (employedetails.branchid = @branchid)");
                //cmd = new SqlCommand("SELECT employedetails.employee_num, employedetails.fullname, employedetails.empid, tds_deduction.tdsamount FROM employedetails INNER JOIN tds_deduction ON employedetails.empid = tds_deduction.empid WHERE (employedetails.branchid = @branchid)");
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dttdsdeduction = vdm.SelectQuery(cmd).Tables[0];
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
                        double otherdeduction = 0;
                        double tdsdeduction = 0;
                        double medicliamdeduction = 0;
                        double mobilededuction = 0;
                        double totaldeduction;
                        double totalearnings;
                        double providentfund = 0;
                        double medicalerning = 0;
                        double washingallowance = 0;
                        double convenyance = 0;
                        double esi = 0;
                        double daysinmonth = 0;
                        double loseamount = 0;
                        double loseofconviyance = 0;
                        double loseofwashing = 0;
                        double loseofmedical = 0;
                        double losofprofitionaltax = 0;
                        DataRow newrow = Report.NewRow();
                        newrow["SNO"] = i++.ToString();
                        //newrow["Employeeid"] = dr["empid"].ToString();
                        newrow["Employee Code"] = dr["employee_num"].ToString();
                        newrow["Name"] = dr["fullname"].ToString();
                        newrow["ledgername"] = dr["ledgername"].ToString();
                        newrow["ledgercode"] = dr["ledgercode"].ToString();
                        newrow["DESIGNATION"] = dr["designation"].ToString();
                        double peryanam = Convert.ToDouble(dr["salaryperyear"].ToString());
                        newrow["GROSS"] = peryanam / 12;
                        double permonth = peryanam / 12;
                        double HRA = Convert.ToDouble(dr["hra"].ToString());
                        double BASIC = Convert.ToDouble(dr["erningbasic"].ToString());
                        convenyance = Convert.ToDouble(dr["conveyance"].ToString());
                        //newrow["PT"] = dr["profitionaltax"].ToString();
                        profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
                        medicalerning = Convert.ToDouble(dr["medicalerning"].ToString());
                        washingallowance = Convert.ToDouble(dr["washingallowance"].ToString());
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
                                newrow["DAYS MONTH"] = daysinmonth.ToString();
                                double paydays = 0;
                                double lop = 0;
                                double.TryParse(dra["lop"].ToString(), out lop);
                                paydays = numberofworkingdays - lop;
                                newrow["Attendance Days"] = paydays.ToString();
                                double holidays = 0;
                                holidays = daysinmonth - numberofworkingdays;
                                if (lop != 0)
                                {
                                    double totaldays = paydays + clorwo;
                                    newrow["Payable Days"] = totaldays;
                                }
                                else
                                {
                                    newrow["Payable Days"] = paydays + clorwo;
                                }
                                if (clorwo == 0)
                                {
                                }
                                else
                                {
                                    newrow["CL HOLIDAY AND OFF"] = clorwo;
                                }
                                totalpresentdays = holidays + paydays;
                                double totalpdays = permonth / daysinmonth;
                                loseamount = lop * totalpdays;
                                double perdayconveyance = convenyance / daysinmonth;
                                loseofconviyance = lop * perdayconveyance;
                                double perdaywashing = washingallowance / daysinmonth;
                                loseofwashing = lop * perdaywashing;
                                double perdaymedical = medicalerning / daysinmonth;
                                loseofmedical = lop * perdaymedical;
                                double perdaybasic = BASIC / daysinmonth;
                                double perdaprofitionaltax = profitionaltax / daysinmonth;
                                losofprofitionaltax = lop * perdaprofitionaltax;
                            }
                        }
                        double perdaysal = permonth / daysinmonth;
                        double basic = 50;
                        double basicsalary = (permonth * 50) / 100;
                        double basicpermonth = basicsalary / daysinmonth;
                        double bs = basicpermonth * totalpresentdays;
                        newrow["BASIC"] = Math.Round(bs);
                        newrow["CONVEYANCE ALLOWANCE"] = Math.Round(convenyance - loseofconviyance);
                        newrow["MEDICAL ALLOWANCE"] = Math.Round(medicalerning - loseofmedical);
                        newrow["WASHING ALLOWANCE"] = Math.Round(washingallowance - loseofwashing);
                        double basicsal = Math.Round(basicsalary - loseamount);
                        double conve = Math.Round(convenyance - loseofconviyance);
                        double medical = Math.Round(medicalerning - loseofmedical);
                        double washing = Math.Round(washingallowance - loseofwashing);
                        double tt = bs + conve + medical + washing;
                        double thra = permonth - loseamount;
                        double hra = Math.Round(thra - tt);
                        totalearnings = Math.Round(hra + tt);
                        double ptax = 0;
                        if (branchid == 6)
                        {
                            if (totalearnings >= 15000)
                            {
                                ptax = profitionaltax;
                                newrow["PT"] = profitionaltax;
                            }
                            else
                            {
                                ptax = 0;
                                profitionaltax = ptax;
                                newrow["PT"] = ptax;
                            }
                        }
                        else
                        {
                            newrow["PT"] = profitionaltax;
                        }
                        if (hra > 0)
                        {
                            newrow["HRA"] = hra;
                        }
                        else
                        {
                            newrow["HRA"] = 0;
                        }
                        newrow["GROSS Earnings"] = totalearnings;
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
                        string esieligible = dr["esieligible"].ToString();
                        if (esieligible == "Yes")
                        {
                            esi = (totalearnings * 1.75) / 100;
                            esi = Math.Round(esi, 0);
                            newrow["ESI"] = esi;
                        }
                        else
                        {
                            esi = 0;
                            newrow["ESI"] = esi;
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
                                    if (amount == 0)
                                    {
                                    }
                                    else
                                    {
                                        newrow["SALARY ADVANCE"] = amount.ToString();
                                        salaryadvance = Convert.ToDouble(amount);
                                        salaryadvance = Math.Round(amount, 0);
                                    }
                                }
                            }
                            else
                            {
                                salaryadvance = 0;
                                if (salaryadvance == 0)
                                {
                                    newrow["SALARY ADVANCE"] = salaryadvance;
                                }
                                else
                                {
                                    newrow["SALARY ADVANCE"] = salaryadvance;
                                }
                            }
                        }
                        else
                        {
                            salaryadvance = 0;
                            if (salaryadvance == 0)
                            {
                                newrow["SALARY ADVANCE"] = salaryadvance;
                            }
                            else
                            {
                                newrow["SALARY ADVANCE"] = salaryadvance;
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
                                    if (loanemimonth == 0)
                                    {
                                    }
                                    else
                                    {
                                        newrow["Loan"] = loanemimonth.ToString();
                                        loan = Convert.ToDouble(loanemimonth);
                                        loan = Math.Round(loanemimonth, 0);
                                    }
                                }
                            }
                            else
                            {
                                loan = 0;
                                if (loan == 0)
                                {
                                }
                                else
                                {
                                    newrow["Loan"] = loan;
                                }
                            }
                        }
                        else
                        {
                            loan = 0;
                            if (loan == 0)
                            {
                            }
                            else
                            {
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
                        else
                        {
                            mobilededuction = 0;
                            newrow["MOBILE DEDUCTION"] = mobilededuction;
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
                                    if (deductionamount == 0)
                                    {
                                        newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                    }
                                    else
                                    {
                                        newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                        string st = deductionamount.ToString();
                                        canteendeduction = Convert.ToDouble(deductionamount);
                                        canteendeduction = Math.Round(deductionamount);
                                    }
                                }
                            }
                            else
                            {
                                canteendeduction = 0;
                                if (canteendeduction == 0)
                                {
                                    newrow["CANTEEN DEDUCTION"] = canteendeduction;
                                }
                                else
                                {
                                    newrow["CANTEEN DEDUCTION"] = canteendeduction;
                                }
                            }
                        }
                        else
                        {
                            canteendeduction = 0;
                            if (canteendeduction == 0)
                            {
                                newrow["CANTEEN DEDUCTION"] = canteendeduction;
                            }
                            else
                            {
                                newrow["CANTEEN DEDUCTION"] = canteendeduction;
                            }
                        }
                        medicliamdeduction = 0;
                        if (dtmedicliam.Rows.Count > 0)
                        {
                            DataRow[] drr = dtmedicliam.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                            if (drr.Length > 0)
                            {
                                foreach (DataRow drmedicliam in dtmedicliam.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                {
                                    double amount = 0;
                                    double.TryParse(drmedicliam["medicliamamount"].ToString(), out amount);
                                    newrow["MEDICLAIM DEDUCTION"] = amount.ToString();
                                    string st = amount.ToString();
                                    medicliamdeduction = Convert.ToDouble(amount);
                                    medicliamdeduction = Math.Round(amount, 0);
                                }
                            }
                            else
                            {
                                medicliamdeduction = 0;
                                newrow["MEDICLAIM DEDUCTION"] = medicliamdeduction;
                            }
                        }
                        else
                        {
                            medicliamdeduction = 0;
                            newrow["MEDICLAIM DEDUCTION"] = medicliamdeduction;
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
                                        newrow["OTHER DEDUCTION"] = amount.ToString();
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
                                    newrow["OTHER DEDUCTION"] = otherdeduction;
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
                                newrow["OTHER DEDUCTION"] = otherdeduction;
                            }
                            else
                            {
                                //otherdeduction = 0;
                                newrow["OTHER DEDUCTION"] = otherdeduction;
                            }
                        }
                        tdsdeduction = 0;
                        if (dttdsdeduction.Rows.Count > 0)
                        {
                            DataRow[] drr = dttdsdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'");
                            if (drr.Length > 0)
                            {
                                foreach (DataRow drtdsdeduction in dttdsdeduction.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                                {
                                    double amount = 0;
                                    double.TryParse(drtdsdeduction["tdsamount"].ToString(), out amount);
                                    if (amount == 0)
                                    {
                                        newrow["Tds DEDUCTION"] = amount.ToString();
                                    }
                                    else
                                    {
                                        newrow["Tds DEDUCTION"] = amount.ToString();
                                        string st = amount.ToString();
                                        tdsdeduction = Convert.ToDouble(amount);
                                        tdsdeduction = Math.Round(amount, 0);
                                    }
                                }
                            }
                            else
                            {
                                tdsdeduction = 0;
                                if (tdsdeduction == 0)
                                {
                                    newrow["Tds DEDUCTION"] = tdsdeduction;
                                }
                                else
                                {

                                    newrow["Tds DEDUCTION"] = tdsdeduction;
                                }
                            }
                        }
                        else
                        {
                            tdsdeduction = 0;
                            if (tdsdeduction == 0)
                            {
                                newrow["Tds DEDUCTION"] = tdsdeduction;
                            }
                            else
                            {
                                newrow["Tds DEDUCTION"] = tdsdeduction;
                            }
                        }
                        newrow["TOTAL DEDUCTIONS"] = profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + providentfund + esi + medicliamdeduction + otherdeduction + tdsdeduction;
                        totaldeduction = profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + providentfund + esi + medicliamdeduction + otherdeduction + tdsdeduction;
                        double netpay = 0;
                        netpay = totalearnings - totaldeduction;
                       // netpay = Math.Round(netpay);
                        //netpay = netpay;
                        newrow["NET PAY"] = netpay;
                        Report.Rows.Add(newrow);
                    }
                }
            }
            else if (ddlemptype.SelectedItem.Text == "Canteen")
            {
                  double totalearnings = 0;
                  Report.Columns.Add("SNO");
                  Report.Columns.Add("Employee Code");
                  Report.Columns.Add("Name");
                  Report.Columns.Add("ledgername");
                  Report.Columns.Add("ledgercode");
                  Report.Columns.Add("DESIGNATION");
                  Report.Columns.Add("GROSS").DataType = typeof(double);
                  Report.Columns.Add("DAYS MONTH").DataType = typeof(double);
                  Report.Columns.Add("Attendance Days").DataType = typeof(double);
                  Report.Columns.Add("CL HOLIDAY AND OFF").DataType = typeof(double);
                  Report.Columns.Add("Payable Days").DataType = typeof(double);
                  Report.Columns.Add("BASIC").DataType = typeof(double);
                  Report.Columns.Add("HRA").DataType = typeof(double);
                  Report.Columns.Add("CONVEYANCE ALLOWANCE").DataType = typeof(double);
                  Report.Columns.Add("MEDICAL ALLOWANCE").DataType = typeof(double);
                  Report.Columns.Add("WASHING ALLOWANCE").DataType = typeof(double);
                  Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                  Report.Columns.Add("PT").DataType = typeof(double);
                  Report.Columns.Add("PF").DataType = typeof(double);
                  Report.Columns.Add("ESI").DataType = typeof(double);
                  Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                  Report.Columns.Add("Loan").DataType = typeof(double);
                  Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                  Report.Columns.Add("MOBILE DEDUCTION").DataType = typeof(double);
                  Report.Columns.Add("MEDICLAIM DEDUCTION").DataType = typeof(double);
                  Report.Columns.Add("Tds DEDUCTION").DataType = typeof(double);
                  Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                  Report.Columns.Add("TOTAL DEDUCTIONS").DataType = typeof(double);
                  Report.Columns.Add("NET PAY").DataType = typeof(double);
                  Report.Columns.Add("Bank Acc NO");
                  Report.Columns.Add("IFSC Code");
                  Report.Columns.Add("Extra DAYS");
                  Report.Columns.Add("Extra DAYS Value");
                  Report.Columns.Add("Total Salary");
                 

                int branchid = Convert.ToInt32(ddlbranches.SelectedItem.Value);
                string employee_type = ddlemptype.SelectedItem.Value;
                //paystrutre
                // cmd = new SqlCommand("SELECT  employedetails.pfeligible, employebankdetails.ifsccode, employedetails.esieligible, employedetails.employee_type, employedetails.empid,  employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.gross, pay_structure.hra, pay_structure.conveyance, pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno, monthly_attendance.month, monthly_attendance.year,  employedetails.employee_dept FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN  monthly_attendance ON employedetails.empid = monthly_attendance.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year)");
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employebankdetails.ifsccode, employedetails.esieligible, employedetails.employee_type, employedetails.empid, employedetails.employee_num, employedetails.fullname, designation.designation, employebankdetails.accountno, monthly_attendance.month,  monthly_attendance.year, employedetails.employee_dept, salaryappraisals.gross, salaryappraisals.erningbasic, salaryappraisals.hra, salaryappraisals.profitionaltax, salaryappraisals.esi, salaryappraisals.providentfund, salaryappraisals.conveyance, salaryappraisals.medicalerning, salaryappraisals.washingallowance, salaryappraisals.travelconveyance, salaryappraisals.salaryperyear FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN salaryappraisals ON employedetails.empid = salaryappraisals.empid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (salaryappraisals.endingdate IS NULL) AND (salaryappraisals.startingdate <= @d1) OR (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (employedetails.branchid = @branchid) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (salaryappraisals.endingdate > @d1) AND (salaryappraisals.startingdate <= @d1)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@emptype", employee_type);
                cmd.Parameters.Add("@d1", date);
                DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid,monthly_attendance.clorwo,monthly_attendance.extradays, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN  employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid WHERE  (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                //cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_workingdays.numberofworkingdays, monthly_attendance.lop,  branchmaster.branchid FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN monthly_workingdays ON branchmaster.branchid = monthly_workingdays.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid=@branchid)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT employedetails.employee_num, loan_request.loanemimonth, loan_request.month FROM employedetails INNER JOIN  loan_request ON employedetails.empid = loan_request.empid where (employedetails.branchid=@branchid) AND (loan_request.month = @month) AND (loan_request.year = @year)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtloan = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) AND (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT monthly_attendance.sno, employedetails.employee_num, employedetails.fullname, monthly_attendance.empid, monthly_attendance.doe,  monthly_attendance.month, monthly_attendance.year, monthly_attendance.extradays, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays, pay_structure.gross, employebankdetails.employeid, employebankdetails.accountno,  designation.designation FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN designation ON employedetails.designationid = designation.designationid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                DataTable dtOT = SalesDB.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT  employedetails.employee_num, canteendeductions.amount, employedetails.fullname, employedetails.empid FROM  canteendeductions INNER JOIN employedetails ON canteendeductions.employee_num = employedetails.employee_num where (employedetails.branchid=@branchid)AND (canteendeductions.month = @month) AND (canteendeductions.year = @year)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                DataTable dtcanteen = vdm.SelectQuery(cmd).Tables[0];
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
                        double daysinmonth = 0;
                        double loseamount = 0;
                        double otamount = 0;
                        DataRow newrow = Report.NewRow();
                        newrow["SNO"] = i++.ToString();
                        newrow["Employee Code"] = dr["employee_num"].ToString();
                        newrow["Name"] = dr["fullname"].ToString();
                        newrow["DESIGNATION"] = dr["designation"].ToString();
                        double BASIC = Convert.ToDouble(dr["erningbasic"].ToString());
                       // profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
                        newrow["GROSS"] = dr["gross"].ToString();
                        newrow["Bank Acc NO"] = dr["accountno"].ToString();
                        newrow["IFSC Code"] = dr["ifsccode"].ToString();
                        //double permonth = peryanam / 12;
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
                                newrow["Attendance days"] = daysinmonth.ToString();
                                //double rateperday = 0;
                                double paydays = 0;
                                double lop = 0;
                                double.TryParse(dra["lop"].ToString(), out lop);
                                paydays = daysinmonth - lop;
                                double holidays = 0;
                                holidays = daysinmonth - numberofworkingdays;
                                newrow["Payable Days"] = paydays;
                                double monthsal = Convert.ToDouble(dr["gross"].ToString());
                                //newrow["Rate/Day"] = rate;
                                totalpresentdays = holidays + paydays;
                               // double perdaysal = permonth / daysinmonth;
                                //totalearnings = rate * paydays;
                                double perdayamt = monthsal / daysinmonth;
                                //double otvalue = perdayamt * otdays;
                                 totalearnings = perdayamt * paydays;
                                totalearnings = Math.Round(totalearnings);
                                newrow["Gross Earnings"] = totalearnings;
                                if (clorwo == 0)
                                {
                                }
                                else
                                {
                                    newrow["CL HOLIDAY AND OFF"] = clorwo;
                                }
                            }
                        }
                        foreach (DataRow drot in dtOT.Select("employee_num='" + dr["employee_num"].ToString() + "'"))
                        {

                            string ot = drot["extradays"].ToString();
                            double numberofworkingdays = 0;
                            double.TryParse(drot["numberofworkingdays"].ToString(), out numberofworkingdays);
                            double otdays = 0;
                            if (numberofworkingdays == 0.0)
                            {
                                numberofworkingdays = 1;
                            }
                            if (ot == null || ot == "")
                            {
                                otdays = 0;
                            }
                            else
                            {
                                otdays = Convert.ToDouble(drot["extradays"].ToString());
                            }
                            double monthsal = Convert.ToDouble(drot["gross"].ToString());
                            double perdayamt = monthsal / daysinmonth;
                            double otvalue = perdayamt * otdays;
                            newrow["Extra DAYS"] = otdays;
                            newrow["Extra DAYS Value"] = Math.Round(otvalue);
                            otamount = otvalue;
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
                                    newrow["SALARY ADVANCE"] = amount.ToString();
                                    salaryadvance = Convert.ToDouble(amount);
                                    salaryadvance = Math.Round(amount);
                                }
                            }
                            else
                            {
                                salaryadvance = 0;
                                newrow["SALARY ADVANCE"] = salaryadvance;
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
                                    //newrow["CANTEEN DEDUCTION"] = deductionamount.ToString();
                                    string st = deductionamount.ToString();
                                    canteendeduction = Convert.ToDouble(deductionamount);
                                    canteendeduction = Math.Round(deductionamount, 0);
                                }
                            }
                            else
                            {
                                canteendeduction = 0;
                                //newrow["CANTEEN DEDUCTION"] = canteendeduction;
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
                        newrow["TOTAL DEDUCTIONS"] = Math.Round(canteendeduction + salaryadvance + loan);
                        totaldeduction = Math.Round(canteendeduction + salaryadvance + loan);
                        double netpay = 0;
                        double netpayextpay = 0;
                        netpayextpay = Math.Round(totalearnings - totaldeduction + otamount);
                        netpay = Math.Round(totalearnings - totaldeduction );
                        netpay = Math.Round(netpay, 2);
                        newrow["NET PAY"] = Math.Round(netpay, 0);
                        newrow["Total Salary"] = Math.Round(netpayextpay, 0);
                    }
                }
            }
            else if (ddlemptype.SelectedItem.Text == "Driver")
            {
                double totalearnings = 0;
                Report.Columns.Add("SNO");
                // Report.Columns.Add("Employeeid");
                Report.Columns.Add("Employee Code");
                Report.Columns.Add("Name");
                Report.Columns.Add("ledgername");
                Report.Columns.Add("ledgercode");
                Report.Columns.Add("DESIGNATION");
                Report.Columns.Add("Rate/Day");
                Report.Columns.Add("Batta/Day");
                Report.Columns.Add("Work Days").DataType = typeof(double);
                //Report.Columns.Add("Attendance Days").DataType = typeof(double);
                //Report.Columns.Add("CL HOLIDAY AND OFF").DataType = typeof(double);
                Report.Columns.Add("Payable Days").DataType = typeof(double);
                Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                Report.Columns.Add("PT").DataType = typeof(double);
                Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                Report.Columns.Add("Loan").DataType = typeof(double);
                Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("MOBILE DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("TOTAL DEDUCTIONS").DataType = typeof(double);
                Report.Columns.Add("NET PAY").DataType = typeof(double);
                Report.Columns.Add("Bank Acc NO");
                Report.Columns.Add("IFSC Code");
                int branchid = Convert.ToInt32(ddlbranches.SelectedItem.Value);
                string employee_type = ddlemptype.SelectedItem.Value;
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employedetails.pfdate, employedetails.ledgercode, employedetails.ledgername,employedetails.esidate, employebankdetails.ifsccode, employedetails.esieligible, monthly_attendance.month, monthly_attendance.year,employedetails.employee_type, employedetails.empid, employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.gross, pay_structure.hra, pay_structure.conveyance, pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno, branchmaster.statename FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@emptype", employee_type);
                DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid,monthly_attendance.clorwo, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN  employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid WHERE  (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                //cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_workingdays.numberofworkingdays, monthly_attendance.lop,  branchmaster.branchid FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN monthly_workingdays ON branchmaster.branchid = monthly_workingdays.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid=@branchid)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) AND (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT employedetails.employee_num, loan_request.loanemimonth, loan_request.months, loan_request.startdate FROM employedetails INNER JOIN  loan_request ON employedetails.empid = loan_request.empid where (employedetails.branchid=@branchid) AND (loan_request.month = @month) AND (loan_request.year = @year)");
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
                        //double totalearnings;
                        double daysinmonth = 0;
                        string statename = "";
                        DataRow newrow = Report.NewRow();
                        newrow["SNO"] = i++.ToString();
                        //newrow["Employeeid"] = dr["empid"].ToString();
                        newrow["Employee Code"] = dr["employee_num"].ToString();
                        newrow["Name"] = dr["fullname"].ToString();
                        newrow["ledgername"] = dr["ledgername"].ToString();
                        newrow["ledgercode"] = dr["ledgercode"].ToString();
                        newrow["DESIGNATION"] = dr["designation"].ToString();
                        double BASIC = Convert.ToDouble(dr["erningbasic"].ToString());
                        statename = dr["statename"].ToString();
                        //newrow["PT"] = dr["profitionaltax"].ToString();
                        double rateper = Convert.ToDouble(dr["gross"].ToString());
                        //profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
                        newrow["Bank Acc NO"] = dr["accountno"].ToString();
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
                                newrow["Work Days"] = daysinmonth.ToString();
                                //double rateperday = 0;
                                double paydays = 0;
                                double lop = 0;
                                double.TryParse(dra["lop"].ToString(), out lop);
                                paydays = daysinmonth - lop;
                                double holidays = 0;
                                holidays = daysinmonth - numberofworkingdays;
                                newrow["Payable Days"] = paydays;
                                totalpresentdays = holidays + paydays;
                                //double perdaybasic = BASIC / daysinmonth;
                                double rate = Convert.ToDouble(dr["gross"].ToString());
                                newrow["Rate/Day"] = rateper;
                                double batta = 80;
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
                                double rateperday = rate + batta;
                                totalearnings = rateperday * paydays;
                                double totalpdays = numberofworkingdays - lop;
                                totalearnings = Math.Round(totalearnings);
                                newrow["GROSS Earnings"] = totalearnings;
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
                        newrow["TOTAL DEDUCTIONS"] = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + otherdeduction);
                        totaldeduction = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction + otherdeduction);
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
                            newrow["NET PAY"] = Math.Round(netpay, 0);
                            Report.Rows.Add(newrow);
                        }
                        //newrow["NET PAY"] = Math.Ceiling(netpay);
                    }
                }
            }
            else if (ddlemptype.SelectedItem.Text == "Cleaner")
            {
                double totalearnings = 0;
                Report.Columns.Add("SNO");
                // Report.Columns.Add("Employeeid");
                Report.Columns.Add("Employee Code");
                Report.Columns.Add("Name");
                Report.Columns.Add("ledgername");
                Report.Columns.Add("ledgercode");
                Report.Columns.Add("DESIGNATION");
                Report.Columns.Add("Rate/Day");
                Report.Columns.Add("Batta/Day");
                Report.Columns.Add("Work Days").DataType = typeof(double);
                //Report.Columns.Add("Attendance Days").DataType = typeof(double);
                //Report.Columns.Add("CL HOLIDAY AND OFF").DataType = typeof(double);
                Report.Columns.Add("Payable Days").DataType = typeof(double);
                Report.Columns.Add("GROSS Earnings").DataType = typeof(double);
                Report.Columns.Add("PT").DataType = typeof(double);
                Report.Columns.Add("SALARY ADVANCE").DataType = typeof(double);
                Report.Columns.Add("Loan").DataType = typeof(double);
                Report.Columns.Add("CANTEEN DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("MOBILE DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("OTHER DEDUCTION").DataType = typeof(double);
                Report.Columns.Add("TOTAL DEDUCTIONS").DataType = typeof(double);
                Report.Columns.Add("NET PAY").DataType = typeof(double);
                Report.Columns.Add("Bank Acc NO");
                Report.Columns.Add("IFSC Code");
                int branchid = Convert.ToInt32(ddlbranches.SelectedItem.Value);
                string employee_type = ddlemptype.SelectedItem.Value;
                cmd = new SqlCommand("SELECT employedetails.pfeligible, employebankdetails.ifsccode, employedetails.ledgercode, employedetails.ledgername,employedetails.esieligible, monthly_attendance.month, monthly_attendance.year,employedetails.employee_type, employedetails.empid, employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.gross, pay_structure.hra, pay_structure.conveyance, pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno, branchmaster.statename FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN monthly_attendance ON employedetails.empid = monthly_attendance.empid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid = @branchid) AND (employedetails.status = 'No') AND (employedetails.employee_type = @emptype) AND (monthly_attendance.month = @month) AND (monthly_attendance.year = @year)");
                //cmd = new SqlCommand("SELECT employedetails.pfeligible, employebankdetails.ifsccode, employedetails.esieligible,employedetails.employee_type, employedetails.empid,employedetails.employee_num, pay_structure.erningbasic, pay_structure.esi, pay_structure.providentfund, employedetails.fullname, designation.designation, pay_structure.gross, pay_structure.hra, pay_structure.conveyance,  pay_structure.washingallowance, pay_structure.medicalerning, pay_structure.profitionaltax, employebankdetails.accountno FROM employedetails INNER JOIN designation ON employedetails.designationid = designation.designationid INNER JOIN pay_structure ON employedetails.empid = pay_structure.empid  LEFT OUTER JOIN employebankdetails ON employedetails.empid = employebankdetails.employeid WHERE (employedetails.branchid=@branchid) and (employedetails.status='No') and (employedetails.employee_type=@emptype)");
                cmd.Parameters.Add("@branchid", branchid);
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@emptype", employee_type);
                DataTable dtsalary = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid, monthly_attendance.clorwo, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_attendance.lop, branchmaster.branchid, monthly_attendance.numberofworkingdays FROM  monthly_attendance INNER JOIN  employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid WHERE  (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid = @branchid)");
                //cmd = new SqlCommand("SELECT monthly_attendance.sno, monthly_attendance.empid, monthly_attendance.doe, monthly_attendance.month, monthly_attendance.year, monthly_attendance.otdays, employedetails.employee_num, branchmaster.fromdate, branchmaster.todate, monthly_workingdays.numberofworkingdays, monthly_attendance.lop,  branchmaster.branchid FROM  monthly_attendance INNER JOIN employedetails ON monthly_attendance.empid = employedetails.empid INNER JOIN branchmaster ON employedetails.branchid = branchmaster.branchid INNER JOIN monthly_workingdays ON branchmaster.branchid = monthly_workingdays.branchid WHERE (monthly_attendance.month = @month) AND (monthly_attendance.year = @year) AND (employedetails.branchid=@branchid)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtattendence = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT  employedetails.employee_num, salaryadvance.amount, salaryadvance.monthofpaid, employedetails.fullname, salaryadvance.empid FROM   salaryadvance INNER JOIN  employedetails ON salaryadvance.empid = employedetails.empid where (employedetails.branchid=@branchid) AND (salaryadvance.month = @month) AND (salaryadvance.year = @year)");
                cmd.Parameters.Add("@month", mymonth);
                cmd.Parameters.Add("@year", year);
                cmd.Parameters.Add("@branchid", branchid);
                DataTable dtsa = vdm.SelectQuery(cmd).Tables[0];
                cmd = new SqlCommand("SELECT employedetails.employee_num, loan_request.loanemimonth, loan_request.months, loan_request.startdate FROM employedetails INNER JOIN  loan_request ON employedetails.empid = loan_request.empid where (employedetails.branchid=@branchid) AND (loan_request.month = @month) AND (loan_request.year = @year)");
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
                        //double totalearnings;
                        double daysinmonth = 0;
                        double loseamount = 0;
                        DataRow newrow = Report.NewRow();
                        newrow["SNO"] = i++.ToString();
                        //newrow["Employeeid"] = dr["empid"].ToString();
                        newrow["Employee Code"] = dr["employee_num"].ToString();
                        newrow["Name"] = dr["fullname"].ToString();
                        newrow["ledgername"] = dr["ledgername"].ToString();
                        newrow["ledgercode"] = dr["ledgercode"].ToString();
                        newrow["DESIGNATION"] = dr["designation"].ToString();
                        double BASIC = Convert.ToDouble(dr["erningbasic"].ToString());
                        newrow["PT"] = dr["profitionaltax"].ToString();
                        double rateper = Convert.ToDouble(dr["gross"].ToString());
                        profitionaltax = Convert.ToDouble(dr["profitionaltax"].ToString());
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
                                newrow["Work Days"] = daysinmonth.ToString();
                                double paydays = 0;
                                double lop = 0;
                                double.TryParse(dra["lop"].ToString(), out lop);
                                paydays = daysinmonth - lop;
                                double holidays = 0;
                                holidays = daysinmonth - numberofworkingdays;
                                newrow["Payable Days"] = paydays;
                                double rate = Convert.ToDouble(dr["gross"].ToString());
                                newrow["Rate/Day"] = rateper;
                                double batta = 80;
                                newrow["Batta/Day"] = batta;
                                double rateperday = rate + batta;
                                totalearnings = rateperday * paydays;
                                double totalpdays = numberofworkingdays - lop;
                                totalearnings = Math.Round(totalearnings);
                                newrow["GROSS Earnings"] = totalearnings;
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
                                        mobilededuction = Math.Ceiling(deductionamount);
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
                        newrow["TOTAL DEDUCTIONS"] = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction);
                        totaldeduction = Math.Round(profitionaltax + canteendeduction + salaryadvance + loan + mobilededuction);
                        double netpay = 0;
                        netpay = Math.Round(totalearnings - totaldeduction);
                        netpay = Math.Round(netpay, 2);
                        newrow["NET PAY"] = Math.Round(netpay, 0);
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
            }
            DataRow newTotal = Report.NewRow();
            newTotal["DESIGNATION"] = "Total";
            double val = 0.0;
            foreach (DataColumn dc in Report.Columns)
            {
                if (dc.DataType == typeof(Double))
                {
                    val = 0.0;
                    double.TryParse(Report.Compute("sum([" + dc.ToString() + "])", "[" + dc.ToString() + "]<>'0'").ToString(), out val);
                    if (val == 0)
                    {
                    }
                    else
                    {
                        newTotal[dc.ToString()] = val;
                    }
                }
            }
            Report.Rows.Add(newTotal);
            DataTable dtnew = new DataTable();
            dtnew.Columns.Add("Invoice");
            dtnew.Columns.Add("JV Date");
            dtnew.Columns.Add("Ledger Name");
            dtnew.Columns.Add("Amount");
            dtnew.Columns.Add("Narration");
            string designation = "Total";
            var k = 1;
            string tnarration = "";
            cmd = new SqlCommand("SELECT groupledgername.ledgername, groupledgername.ledgercode, groupledgername.glcodesno, glgroup.glname,glgroup.sno FROM glgroup INNER JOIN groupledgername ON glgroup.sno = groupledgername.glcodesno WHERE (groupledgername.branchid = @BranchID)");
            cmd.Parameters.Add("@BranchID", ddlbranches.SelectedValue);
            DataTable dtledger = vdm.SelectQuery(cmd).Tables[0];
            foreach (DataRow dreport in Report.Select("DESIGNATION='" + designation + "'"))
            {
                DataRow nr_sp = dtnew.NewRow();
                nr_sp["Invoice"] = invoice;
                nr_sp["JV Date"] = talydate;
                string salpayble = "6";
                string sp_ledgr = "";
                foreach (DataRow dr in dtledger.Select("sno='" + salpayble + "'"))
                {
                    sp_ledgr = dr["ledgername"].ToString();
                }
                nr_sp["Ledger Name"] = sp_ledgr;
                double amt = 0;
                double.TryParse(dreport["NET PAY"].ToString(), out amt);
                //nr_sp["Ledger Name"] = "Salary - " + ddlbranches.SelectedItem.Text + "";
                nr_sp["Amount"] = dreport["GROSS Earnings"].ToString();
                tnarration = "Being The SVDS " + ddlbranches.SelectedItem.Text + ddlemptype.SelectedItem.Text + " salaries For the Month Of " + namonth + ", Total Gross=" + dreport["GROSS Earnings"].ToString() + "/-,NetPay=" + amt + "/- ";
                nr_sp["Narration"] = tnarration;
                dtnew.Rows.Add(nr_sp);
            }

            if (Report.Rows.Count > 0)
            {
                foreach (DataRow dr in Report.Rows)
                {
                    if (dr["DESIGNATION"] == "Total")
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


            // starts person tds, medicliem, other dedection.


            if (Report.Rows.Count > 0)
            {
                string myString = ddlmonth.SelectedItem.Text;
                myString = myString.Substring(0, 3);
                foreach (DataRow dr in Report.Rows)
                {
                    if (dr["DESIGNATION"] == "Total")
                    {
                    }
                    else
                    {
                        // PT
                        string name = dr["Name"].ToString();
                        string PT = dr["PT"].ToString();
                        if (PT != "0")
                        {
                            DataRow nr_spt = dtnew.NewRow();
                            invoice = "" + name + " PT " + myString + "-" + year + "";
                            nr_spt["Invoice"] = invoice;
                            nr_spt["JV Date"] = talydate;
                            nr_spt["Ledger Name"] = dr["ledgername"].ToString();
                            nr_spt["Amount"] = dr["PT"].ToString();
                            nr_spt["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_spt);
                            string Ptpayble = "5";
                            string pt_ledgr = "";
                            foreach (DataRow drl in dtledger.Select("sno='" + Ptpayble + "'"))
                            {
                                pt_ledgr = drl["ledgername"].ToString();
                                DataRow nr_spl = dtnew.NewRow();
                                nr_spl["Invoice"] = invoice;
                                nr_spl["JV Date"] = talydate;
                                nr_spl["Ledger Name"] = pt_ledgr;
                                nr_spl["Amount"] = "-" + dr["PT"].ToString() + "";
                                nr_spl["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_spl);
                            }
                        }

                        // MEDICLIEM
                        string MEDICLAIM = dr["MEDICLAIM DEDUCTION"].ToString();
                        if (MEDICLAIM != "0")
                        {
                            DataRow nr_Lmedi = dtnew.NewRow();
                            invoice = "" + name + " Medicliam " + myString + "-" + year + "";
                            nr_Lmedi["Invoice"] = invoice;
                            nr_Lmedi["JV Date"] = talydate;
                            nr_Lmedi["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Lmedi["Amount"] = dr["MEDICLAIM DEDUCTION"].ToString();
                            nr_Lmedi["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Lmedi);
                            string medicliampayble = "2";
                            string medicl_lger = "";
                            foreach (DataRow drmedi in dtledger.Select("sno='" + medicliampayble + "'"))
                            {
                                medicl_lger = drmedi["ledgername"].ToString();
                                DataRow nr_smedi = dtnew.NewRow();
                                nr_smedi["Invoice"] = invoice;
                                nr_smedi["JV Date"] = talydate;
                                nr_smedi["Ledger Name"] = medicl_lger;
                                nr_smedi["Amount"] = "-" + dr["MEDICLAIM DEDUCTION"].ToString() + "";
                                nr_smedi["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_smedi);
                            }
                        }

                        // TDS
                        string Tds = dr["Tds DEDUCTION"].ToString();
                        if (Tds != "0")
                        {
                            DataRow nr_Ltds = dtnew.NewRow();
                            invoice = "" + name + " Tds " + myString + "-" + year + "";
                            nr_Ltds["Invoice"] = invoice;
                            nr_Ltds["JV Date"] = talydate;
                            nr_Ltds["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Ltds["Amount"] = dr["Tds DEDUCTION"].ToString();
                            nr_Ltds["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Ltds);
                            string tdspayble = "3";
                            string tds_ledger = "";
                            foreach (DataRow drtds in dtledger.Select("sno='" + tdspayble + "'"))
                            {
                                tds_ledger = drtds["ledgername"].ToString();
                                DataRow nr_stds = dtnew.NewRow();
                                nr_stds["Invoice"] = invoice;
                                nr_stds["JV Date"] = talydate;
                                nr_stds["Ledger Name"] = tds_ledger;
                                nr_stds["Amount"] = "-" + dr["Tds DEDUCTION"].ToString() + "";
                                nr_stds["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_stds);
                            }
                        }

                        // MOBILE DEDUCTION
                        string MOBILE = dr["MOBILE DEDUCTION"].ToString();
                        if (MOBILE != "0")
                        {
                            DataRow nr_Ltele = dtnew.NewRow();
                            invoice = "" + name + " Mobile " + myString + "-" + year + "";
                            nr_Ltele["Invoice"] = invoice;
                            nr_Ltele["JV Date"] = talydate;
                            nr_Ltele["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Ltele["Amount"] = dr["MOBILE DEDUCTION"].ToString();
                            nr_Ltele["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Ltele);
                            string telphonepayble = "4";
                            string telphone_ledger = "";
                            foreach (DataRow drtele in dtledger.Select("sno='" + telphonepayble + "'"))
                            {
                                telphone_ledger = drtele["ledgername"].ToString();
                                DataRow nr_stele = dtnew.NewRow();
                                nr_stele["Invoice"] = invoice;
                                nr_stele["JV Date"] = talydate;
                                nr_stele["Ledger Name"] = telphone_ledger;
                                nr_stele["Amount"] = "-" + dr["MOBILE DEDUCTION"].ToString() + "";
                                nr_stele["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_stele);
                            }
                        }

                        string OTHER = dr["OTHER DEDUCTION"].ToString();
                        if (OTHER != "0")
                        {
                            // Other DEDUCTION
                            DataRow nr_Lother = dtnew.NewRow();
                            invoice = "" + name + " OTHER " + myString + "-" + year + "";
                            nr_Lother["Invoice"] = invoice;
                            nr_Lother["JV Date"] = talydate;
                            nr_Lother["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Lother["Amount"] = dr["OTHER DEDUCTION"].ToString();
                            nr_Lother["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Lother);
                            string othdeduction = "6";
                            string otherdeduction_ledger = "";
                            foreach (DataRow drother in dtledger.Select("sno='" + othdeduction + "'"))
                            {
                                otherdeduction_ledger = drother["ledgername"].ToString();
                                DataRow nr_sother = dtnew.NewRow();
                                nr_sother["Invoice"] = invoice;
                                nr_sother["JV Date"] = talydate;
                                nr_sother["Ledger Name"] = otherdeduction_ledger;
                                nr_sother["Amount"] = "-" + dr["OTHER DEDUCTION"].ToString() + "";
                                nr_sother["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_sother);
                            }
                        }



                        // Canteen DEDUCTION
                        string CANTEEN = dr["CANTEEN DEDUCTION"].ToString();
                        if (CANTEEN != "0")
                        {
                            DataRow nr_Lcanten = dtnew.NewRow();
                            invoice = "" + name + " Mess " + myString + "-" + year + "";
                            nr_Lcanten["Invoice"] = invoice;
                            nr_Lcanten["JV Date"] = talydate;
                            nr_Lcanten["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Lcanten["Amount"] = dr["CANTEEN DEDUCTION"].ToString();
                            nr_Lcanten["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Lcanten);
                            string messpayble = "15";
                            string Mess_ledgr = "";
                            foreach (DataRow drcanteen in dtledger.Select("sno='" + messpayble + "'"))
                            {
                                Mess_ledgr = drcanteen["ledgername"].ToString();
                                DataRow nr_scanteen = dtnew.NewRow();
                                nr_scanteen["Invoice"] = invoice;
                                nr_scanteen["JV Date"] = talydate;
                                nr_scanteen["Ledger Name"] = Mess_ledgr;
                                nr_scanteen["Amount"] = "-" + dr["CANTEEN DEDUCTION"].ToString() + "";
                                nr_scanteen["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_scanteen);
                            }
                        }



                        // PF DEDUCTION
                        string PF = dr["PF"].ToString();
                        if (PF != "0")
                        {
                            DataRow nr_LPF = dtnew.NewRow();
                            invoice = "" + name + " pf " + myString + "-" + year + "";
                            nr_LPF["Invoice"] = invoice;
                            nr_LPF["JV Date"] = talydate;
                            nr_LPF["Ledger Name"] = dr["ledgername"].ToString();
                            nr_LPF["Amount"] = dr["PF"].ToString();
                            nr_LPF["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_LPF);
                            string PFpayble = "13";
                            string PF_ledgr = "";
                            foreach (DataRow drPF in dtledger.Select("sno='" + PFpayble + "'"))
                            {
                                PF_ledgr = drPF["ledgername"].ToString();
                                DataRow nr_spf = dtnew.NewRow();
                                nr_spf["Invoice"] = invoice;
                                nr_spf["JV Date"] = talydate;
                                nr_spf["Ledger Name"] = PF_ledgr;
                                nr_spf["Amount"] = "-" + dr["PF"].ToString() + "";
                                nr_spf["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_spf);
                            }
                        }

                        // ESI DEDUCTION
                        string esi = dr["ESI"].ToString();
                        if (esi != "0")
                        {
                            DataRow nr_Lesi = dtnew.NewRow();
                            invoice = "" + name + " esi " + myString + "-" + year + "";
                            nr_Lesi["Invoice"] = invoice;
                            nr_Lesi["JV Date"] = talydate;
                            nr_Lesi["Ledger Name"] = dr["ledgername"].ToString();
                            nr_Lesi["Amount"] = dr["ESI"].ToString();
                            nr_Lesi["Narration"] = tnarration;
                            dtnew.Rows.Add(nr_Lesi);
                            string esipayble = "14";
                            string ESI_ledgr = "";
                            foreach (DataRow dresi in dtledger.Select("sno='" + esipayble + "'"))
                            {
                                ESI_ledgr = dresi["ledgername"].ToString();
                                DataRow nr_sesi = dtnew.NewRow();
                                nr_sesi["Invoice"] = invoice;
                                nr_sesi["JV Date"] = talydate;
                                nr_sesi["Ledger Name"] = ESI_ledgr;
                                nr_sesi["Amount"] = "-" + dr["ESI"].ToString() + "";
                                nr_sesi["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_sesi);
                            }
                        }

                        // loan DEDUCTION
                        string loan = dr["Loan"].ToString();
                        if (loan == "")
                        {

                        }
                        else
                        {
                            if (loan != "0" || loan != "" | loan != null)
                            {

                                DataRow nr_Lother = dtnew.NewRow();
                                invoice = "" + name + " Loan " + myString + "-" + year + "";
                                nr_Lother["Invoice"] = invoice;
                                nr_Lother["JV Date"] = talydate;
                                nr_Lother["Ledger Name"] = dr["ledgername"].ToString();
                                nr_Lother["Amount"] = dr["Loan"].ToString();
                                nr_Lother["Narration"] = tnarration;
                                dtnew.Rows.Add(nr_Lother);
                                string othdeduction = "6";
                                string otherdeduction_ledger = "";
                                foreach (DataRow drother in dtledger.Select("sno='" + othdeduction + "'"))
                                {
                                    otherdeduction_ledger = drother["ledgername"].ToString();
                                    DataRow nr_sother = dtnew.NewRow();
                                    nr_sother["Invoice"] = invoice;
                                    nr_sother["JV Date"] = talydate;
                                    nr_sother["Ledger Name"] = otherdeduction_ledger;
                                    nr_sother["Amount"] = "-" + dr["Loan"].ToString() + "";
                                    nr_sother["Narration"] = tnarration;
                                    dtnew.Rows.Add(nr_sother);
                                }
                            }
                        }
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
    protected void grdReports_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[2].Text == "Total Amount")
            {
                e.Row.BackColor = System.Drawing.Color.Aquamarine;
                e.Row.Font.Size = FontUnit.Large;
                e.Row.Font.Bold = true;
            }
            //if (e.Row.Cells[3].Text == "Grand Total")
            //{
            //    e.Row.BackColor = System.Drawing.Color.DeepSkyBlue;
            //    e.Row.Font.Size = FontUnit.Large;
            //    e.Row.Font.Bold = true;
            //}
        }
    }
}