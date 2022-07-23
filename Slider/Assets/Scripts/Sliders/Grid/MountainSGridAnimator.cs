using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MountainSGridAnimator : SGridAnimator
{
    public override void Move(SMove move, STile[,] grid = null)
    {
        if (grid == null)
        {
            grid = SGrid.Current.GetGrid();
        }
        Dictionary<Vector2Int, List<int>> borders = move.GenerateBorders();
   
        StartCoroutine(DisableBordersAndColliders(grid, SGrid.Current.GetBGGrid(), move.positions, borders));

        foreach (Movement m in move.moves)
        {
            if (grid[m.startLoc.x, m.startLoc.y].isTileActive)
            {
                if (Mathf.Abs(m.endLoc.y - m.startLoc.y) <= 1) {
                    StartCoroutine(StartMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
                }
                else {
                    StartCoroutine(StartLayerMovingAnimation(grid[m.startLoc.x, m.startLoc.y], m, move));
                }
            }
            else
            {
                grid[m.startLoc.x, m.startLoc.y].SetGridPosition(m.endLoc.x, m.endLoc.y);
            }
        }
    }

    
    // move is only here so we can pass it into the event
    private IEnumerator StartLayerMovingAnimation(STile stile, Movement moveCoords, SMove move)
    {
        //isMoving = true;
        bool isPlayerOnStile = (Player.GetStileUnderneath() != null &&
                                Player.GetStileUnderneath().islandId == stile.islandId);

        stile.SetMovingDirection(GetMovingDirection(moveCoords.startLoc, moveCoords.endLoc));
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(true);
        }

        base.InvokeOnStileMoveStart(stile, moveCoords, move);

        StartCoroutine(StartCameraShakeEffect());

        yield return new WaitForSeconds(movementDuration);

        //isMoving = false;
        
        if (isPlayerOnStile)
        {
            stile.SetBorderColliders(false);
        }

        stile.SetMovingDirection(Vector2.zero);
        stile.SetGridPosition(moveCoords.endLoc);

        base.InvokeOnStileMoveEnd(stile, moveCoords, move);
    }

    protected override Vector2 GetMovingDirection(Vector2 start, Vector2 end) // include magnitude?
    {
        return start - end;
    }
}
