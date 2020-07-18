using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class ScrollViewListener : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
	//滑动方向
	public enum MoveDirection
	{
		None = 0,
		Left,
		Right,
	}
	public float SingleItemWidth;//单个滑动页的宽度
	public RectTransform content;//当前ScrollView的Content
	public float DragMinValue = 5f;//拖动过程中允许的最小的拖拽值，低于此值就不算拖拽，不执行翻页事件
	private MoveDirection direction = MoveDirection.None;
	private int CurIndex = 0;//当前页码
	private int MaxIndex = 0;//最大页码
	public bool canMove = true;//是否能移动
	private Vector3 originalPos;
	private float maxDeltaX = 0f;//取整个拖动过程中的最大值
	public Action<int> OnPageChange;//对外提供页码修改的回调
	/// <summary>
	/// 滑到下一页
	/// </summary>															
	private void MoveToNext()
	{
		if (direction == MoveDirection.Left)
		{
			if (CurIndex < MaxIndex)
			{

				CurIndex++;
				canMove = false;
				content.DOLocalMoveX(content.localPosition.x - SingleItemWidth, 1f).OnComplete(() =>
				{
					if (null != OnPageChange)
					{
						OnPageChange(CurIndex);
					}
					canMove = true;
				});
			}
		}
		else if (direction == MoveDirection.Right)
		{
			if (CurIndex > 0)
			{
				CurIndex--;
				canMove = false;
				content.DOLocalMoveX(content.localPosition.x + SingleItemWidth, 1f).OnComplete(() =>
				{
					if (null != OnPageChange)
					{
						OnPageChange(CurIndex);
					}
					canMove = true;
				});
			}
		}
	}
	/// <summary>
	/// 设置当前滑动列表的页数的最大值
	/// </summary>
	/// <param name="max"></param>
	public void SetMaxIndex(int max)
	{
		MaxIndex = max - 1;//最大下标值为页数减1
	}
	/// <summary>
	/// 设置当前页
	/// </summary>
	/// <param name="index"></param>
	public void SetCurIndex(int index)
	{
		CurIndex = index;
		float x = content.localPosition.x - SingleItemWidth * CurIndex;
		content.localPosition = new Vector3(x, content.localPosition.y, content.localPosition.z);
	}
	public void ResetPosition()
	{
		content.localPosition = originalPos;
	}
	private void Awake()
	{
		CurIndex = 0;
		originalPos = content.localPosition;
	}
	public void OnDrag(PointerEventData eventData)
	{
		if (Mathf.Abs(eventData.delta.x) > maxDeltaX)
		{
			maxDeltaX = Mathf.Abs(eventData.delta.x);
		}
	}
	public void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.delta.x > 0)
		{
			direction = MoveDirection.Right;
		}
		else if (eventData.delta.x < 0)
		{

			direction = MoveDirection.Left;
		}
		else
		{
			direction = MoveDirection.None;
		}
		if (Mathf.Abs(eventData.delta.x) > maxDeltaX)
		{
			maxDeltaX = Mathf.Abs(eventData.delta.x);
		}
	}
	public void OnEndDrag(PointerEventData eventData)
	{
		if (Mathf.Abs(eventData.delta.x) > maxDeltaX)
		{
			maxDeltaX = Mathf.Abs(eventData.delta.x);
		}
		if (maxDeltaX > DragMinValue)
		{
			//计算下一页的目的点 然后移动
			if (canMove)
			{
				MoveToNext();
			}
		}
		maxDeltaX = 0f;
	}
}
