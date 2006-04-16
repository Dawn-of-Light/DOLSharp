using System;

namespace DOL.GS.JumpPoints
{
	/// <summary>
	/// Description résumée de JumpPointTargetLocation.
	/// </summary>
	public class JumpPointTargetLocation
	{
		#region Declaration
		
		/// <summary>
		/// The x coohor of the destination point
		/// </summary>
		protected int m_x;

		/// <summary>
		/// The y coohor of the destination point
		/// </summary>
		protected int m_y;	

		/// <summary>
		/// The z coohor of the destination point
		/// </summary>
		protected int m_z;

		/// <summary>
		/// The heading coohor of the destination point
		/// </summary>
		protected int m_heading;

		/// <summary>
		/// the region id of the destination point
		/// </summary>
		protected int m_region;

		/// <summary>
		/// Get or set the x coohor of the destination point
		/// </summary>
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}

		/// <summary>
		/// Get or set the y coohor of the destination point
		/// </summary>
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}

		/// <summary>
		/// Get or set the z coohor of the destination point
		/// </summary>
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}

		/// <summary>
		/// Get or set the heading coohor of the destination point
		/// </summary>
		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}

		/// <summary>
		/// Gets or sets the region of the destination point
		/// </summary>
		public int Region
		{
			get { return m_region; }
			set { m_region = value; }
		}

		#endregion
	}
}
