using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku
{
    class BoxInformation
    {
        private int m_Row;
        private int m_Col;
        private bool m_Checked;

        public int Row
        {
            get
            {
                return m_Row;
            }

            set
            {
                m_Row = value;
            }
        }

        public int Col
        {
            get
            {
                return m_Col;
            }

            set
            {
                m_Col = value;
            }
        }

        public bool Checked
        {
            get
            {
                return m_Checked;
            }

            set
            {
                m_Checked = value;
            }
        }
    }
}
