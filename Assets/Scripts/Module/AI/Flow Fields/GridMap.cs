using UnityEngine;

namespace FindPath
{
    public class GridMap
    {
        public int width = 30, height = 30;
        public Cell[,] cells;

        public GridMap(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
        /// <summary>
        /// 生成地图
        /// </summary>
        public void Generate()
        {
            cells = new Cell[width, height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    cells[i, j] = new Cell(new Vector2(i, j));
                }
            }
        }

        /// <summary>
        /// 获得上下左右四个格子
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public Cell[] GetNeumanNeighbours(Cell current)
        {
            Cell[] result = new Cell[4];
            int x = (int)current.position.x;
            int y = (int)current.position.y;
            if (y < height - 1)
            {
                result[0] = cells[x, y + 1];
            }
            if (x < width - 1)
            {
                result[1] = cells[x + 1, y];
            }
            if (y > 0)
            {
                result[2] = cells[x, y - 1];
            }
            if (x > 0)
            {
                result[3] = cells[x - 1, y];
            }
            return result;
        }
        //获得8方向版本
        public Cell[] GetMooreNeighbours(Cell current)
        {
            int x = (int)current.position.x;
            int y = (int)current.position.y;
            var result = new Cell[8];
            if (y < height - 1)
                result[0] = cells[x, y + 1];
            if (x < width - 1 && y < height - 1)
                result[1] = cells[x + 1, y + 1];
            if (x < width - 1)
                result[2] = cells[x + 1, y];
            if (x < width - 1 && y > 0)
                result[3] = cells[x + 1, y - 1];
            if (y > 0)
                result[4] = cells[x, y - 1];
            if (x > 0 && y > 0)
                result[5] = cells[x - 1, y - 1];
            if (x > 0)
                result[6] = cells[x - 1, y];
            if (x > 0 && y < height - 1)
                result[7] = cells[x - 1, y + 1];
            return result;
        }
        public Cell FindCellByPosition(Vector2 pos)
        {
            int x = (int)pos.x;
            int y = (int)pos.y;

            return cells[x, y];
        }
    }
}
