namespace TestOpenCVSharp4.Data.DO
{
    public class CamInfoDO
    {
        public int Index { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }

        public List<CamResolutionDO> Resolutions { get; set; } = [];

        public CamInfoDO() { }
        public CamInfoDO(int index, string? name, string? desc)
        {
            Index = index; Name = name; Desc = desc;
        }
    }

    public class CamResolutionDO: IComparable<CamResolutionDO>
    {
        public int W { get; set; }
        public int H { get; set; }
        public int FPS { get; set; }

        public int CompareTo(CamResolutionDO? other)
        {
            if (other == null) return -1;
            if (W == other.W)
            {
                if (H == other.H)
                {
                    return FPS.CompareTo(other.FPS);
                }
                else return H.CompareTo(other.H);
            }
            else return W.CompareTo(other.W);
        }

        public override string ToString()
        {
            return $"{W}x{H}@{FPS}";
        }
    }
}
