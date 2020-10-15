
// MFC_ConsumerView.cpp : implementation of the CMFCConsumerView class
//

#include "pch.h"
#include "framework.h"
// SHARED_HANDLERS can be defined in an ATL project implementing preview, thumbnail
// and search filter handlers and allows sharing of document code with that project.
#ifndef SHARED_HANDLERS
#include "MFC_Consumer.h"
#endif

#include "MFC_ConsumerDoc.h"
#include "MFC_ConsumerView.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CMFCConsumerView

IMPLEMENT_DYNCREATE(CMFCConsumerView, CView)

BEGIN_MESSAGE_MAP(CMFCConsumerView, CView)
	// Standard printing commands
	ON_COMMAND(ID_FILE_PRINT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_DIRECT, &CView::OnFilePrint)
	ON_COMMAND(ID_FILE_PRINT_PREVIEW, &CMFCConsumerView::OnFilePrintPreview)
	ON_WM_CONTEXTMENU()
	ON_WM_RBUTTONUP()
END_MESSAGE_MAP()

// CMFCConsumerView construction/destruction

CMFCConsumerView::CMFCConsumerView() noexcept
{
	// TODO: add construction code here

}

CMFCConsumerView::~CMFCConsumerView()
{
}

BOOL CMFCConsumerView::PreCreateWindow(CREATESTRUCT& cs)
{
	// TODO: Modify the Window class or styles here by modifying
	//  the CREATESTRUCT cs

	return CView::PreCreateWindow(cs);
}

// CMFCConsumerView drawing

void CMFCConsumerView::OnDraw(CDC* /*pDC*/)
{
	CMFCConsumerDoc* pDoc = GetDocument();
	ASSERT_VALID(pDoc);
	if (!pDoc)
		return;

	// TODO: add draw code for native data here
}


// CMFCConsumerView printing


void CMFCConsumerView::OnFilePrintPreview()
{
#ifndef SHARED_HANDLERS
	AFXPrintPreview(this);
#endif
}

BOOL CMFCConsumerView::OnPreparePrinting(CPrintInfo* pInfo)
{
	// default preparation
	return DoPreparePrinting(pInfo);
}

void CMFCConsumerView::OnBeginPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add extra initialization before printing
}

void CMFCConsumerView::OnEndPrinting(CDC* /*pDC*/, CPrintInfo* /*pInfo*/)
{
	// TODO: add cleanup after printing
}

void CMFCConsumerView::OnRButtonUp(UINT /* nFlags */, CPoint point)
{
	ClientToScreen(&point);
	OnContextMenu(this, point);
}

void CMFCConsumerView::OnContextMenu(CWnd* /* pWnd */, CPoint point)
{
#ifndef SHARED_HANDLERS
	theApp.GetContextMenuManager()->ShowPopupMenu(IDR_POPUP_EDIT, point.x, point.y, this, TRUE);
#endif
}


// CMFCConsumerView diagnostics

#ifdef _DEBUG
void CMFCConsumerView::AssertValid() const
{
	CView::AssertValid();
}

void CMFCConsumerView::Dump(CDumpContext& dc) const
{
	CView::Dump(dc);
}

CMFCConsumerDoc* CMFCConsumerView::GetDocument() const // non-debug version is inline
{
	ASSERT(m_pDocument->IsKindOf(RUNTIME_CLASS(CMFCConsumerDoc)));
	return (CMFCConsumerDoc*)m_pDocument;
}
#endif //_DEBUG


// CMFCConsumerView message handlers
