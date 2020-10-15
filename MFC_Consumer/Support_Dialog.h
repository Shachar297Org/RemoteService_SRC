#pragma once


// Support_Dialog dialog

class Support_Dialog : public CDialogEx
{
	DECLARE_DYNAMIC(Support_Dialog)

public:
	Support_Dialog(CWnd* pParent = nullptr);   // standard constructor
	virtual ~Support_Dialog();

// Dialog Data
#ifdef AFX_DESIGN_TIME
	enum { IDD = IDD_DIALOG1 };
#endif

protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	DECLARE_MESSAGE_MAP()
public:
	CEdit m_status;
	afx_msg void OnClickedRequestSupport();
	afx_msg void OnClickedStopSupport();
	afx_msg void OnEnChangeEdit1();
};
