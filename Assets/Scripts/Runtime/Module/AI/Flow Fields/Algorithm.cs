using System.Collections.Generic;
using UnityEngine;

namespace FindPath
{
    public class Algorithm
    {
        private GridMap grid;

        private Vector2[] goals;

        public Algorithm(GridMap grid, Vector2[] goals)
        {
            this.grid = grid;
            this.goals = goals;
        }
        /// <summary>
        /// ����ÿһ����Ԫ��Ŀ�����������
        /// </summary>
        public void GenerateDistance()
        {
            bool[,] isuser = new bool[grid.cells.GetLength(0), grid.cells.GetLength(1)];
            Queue<Cell> marks = new Queue<Cell>();
            for (int i = 0; i < goals.Length; i++)
            {
                var cell = grid.cells[(int)goals[i].x, (int)goals[i].y];
                marks.Enqueue(cell);
            }
            if (goals == null || goals.Length < 1)
            {
                Debug.LogWarning("û������Ŀ���");
            }
            int cost = 0;
            while (marks.Count > 0)
            {
                int count = marks.Count;
                for (int i = 0; i < count; i++)
                {
                    var cell = marks.Dequeue();

                    cell.distance = cost;
                    isuser[(int)cell.position.x, (int)cell.position.y] = true;

                    var neighbours = grid.GetNeumanNeighbours(cell);
                    for (int j = 0; j < neighbours.Length; j++)
                    {
                        var cur = neighbours[j];
                        if (cur != null && isuser[(int)cur.position.x, (int)cur.position.y] == false && cur.UnPassable==false)
                        {
                            marks.Enqueue(cur);
                            isuser[(int)cur.position.x, (int)cur.position.y] = true;
                        }
                    }
                }
                cost++;

            }
        }
        /// <summary>
        /// ����ÿһ����Ԫ��Ŀ���ķ���
        /// </summary>
        public void GenerateVector()
        {
            for (int i = 0; i < grid.cells.GetLength(0); i++)
            {
                for (int j = 0; j < grid.cells.GetLength(1); j++)
                {
                    var cell = grid.cells[i, j];
                    var neighbours = grid.GetMooreNeighbours(cell);
                    Cell min =null;

                    foreach (var c in neighbours)
                    {
                        if (min == null)
                        {
                            min = c;
                        }
                        else if (c !=null &&c.UnPassable==false&& min.distance > c.distance)
                        {
                            min = c;
                        }
                    }
                    cell.direction = min.position-cell.position;
                    cell.direction.Normalize();
                }
            }
            foreach (var i in goals)
            {
                grid.cells[(int)i.x, (int)i.y].direction =Vector2.zero;
                Debug.Log(grid.cells[(int)i.x, (int)i.y].direction);
            }
        }
    }
}
