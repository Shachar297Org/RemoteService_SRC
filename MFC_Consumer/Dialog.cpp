// Dialog.cpp : implementation file
//

#include "pch.h"
#include "MFC_Consumer.h"
#include "Dialog.h"
#include "afxdialogex.h"


// Dialog dialog

IMPLEMENT_DYNAMIC(Dialog, CDialogEx)

Dialog::Dialog(CWnd* pParent /*=nullptr*/)
	: CDialogEx(IDD_DIALOG1, pParent)
	, m_status(_T(""))
{

}

Dialog::~Dialog()
{
}

void Dialog::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_EDIT1, m_status);
}


BEGIN_MESSAGE_MAP(Dialog, CDialogEx)
	ON_BN_CLICKED(IDC_BUTTON1, &Dialog::OnBnClickedButton1)
	ON_BN_CLICKED(IDC_BUTTON2, &Dialog::OnClickStopSupport)
END_MESSAGE_MAP()


// Dialog message handlers


void Dialog::OnBnClickedRequestSupport()
{
	// TODO: Add your control notification handler code here
}


void Dialog::OnClickStopSupport()
{
	// TODO: Add your control notification handler code here
}
