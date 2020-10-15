#pragma once


// Dialog dialog

class Dialog : public CDialogEx
{
	DECLARE_DYNAMIC(Dialog)

public:
	Dialog(CWnd* pParent = nullptr);   // standard constructor
	virtual ~Dialog();

// Dialog Data
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_DIALOG1 };
#endif

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	CString m_status;
//	afx_msg void OnBnClickedButton1();
	afx_msg void OnClickStopSupport();
};
