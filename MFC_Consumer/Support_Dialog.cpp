// Support_Dialog.cpp : implementation file
//

#include "pch.h"
#include "MFC_Consumer.h"
#include "Support_Dialog.h"
#include "afxdialogex.h"
#import "D:\viewstore\ofir.shtainfeld.CORP\alexey.jilinsky_RemoteService_W10_2\RemoteService\RemoteService_SRC\COM\bin\Debug\COM.tlb" raw_interface_only //no_namespace

// Support_Dialog dialog

IMPLEMENT_DYNAMIC(Support_Dialog, CDialogEx)



Support_Dialog::Support_Dialog(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_DIALOG1, pParent)
{
	HRESULT hr = CoInitialize(NULL);
	
}

Support_Dialog::~Support_Dialog()
{
}

void Support_Dialog::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_EDIT1, m_status);
}


BEGIN_MESSAGE_MAP(Support_Dialog, CDialogEx)
	ON_BN_CLICKED(IDC_BUTTON1, &Support_Dialog::OnClickedRequestSupport)
	ON_BN_CLICKED(IDC_BUTTON2, &Support_Dialog::OnClickedStopSupport)
	ON_EN_CHANGE(IDC_EDIT1, &Support_Dialog::OnEnChangeEdit1)
END_MESSAGE_MAP()


// Support_Dialog message handlers


void Support_Dialog::OnClickedRequestSupport()
{
	// TODO: Add your control notification handler code here
	try
	{
		
		COM::IComPtr _com(__uuidof(COM::ComLib));
		_com->RequestSupport();
	}
	catch (...)
	{

	}
}


void Support_Dialog::OnClickedStopSupport()
{
	// TODO: Add your control notification handler code here
	try
	{
		//status = "start";
		//GetDlgCtrlID(IDC_EDIT1);
		//SetWindowTextW()
	}
	catch (...)
	{

	}
}


void Support_Dialog::OnEnChangeEdit1()
{
	// TODO:  If this is a RICHEDIT control, the control will not
	// send this notification unless you override the CDialogEx::OnInitDialog()
	// function and call CRichEditCtrl().SetEventMask()
	// with the ENM_CHANGE flag ORed into the mask.

	// TODO:  Add your control notification handler code here
}
