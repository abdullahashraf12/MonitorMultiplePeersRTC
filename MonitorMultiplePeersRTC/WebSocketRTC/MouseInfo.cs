namespace MonitorMultiplePeersRTC.WebSocketRTC
{
    public class MouseInfo
    {
       
            public double X { get; set; } // Mouse X position in the div
            public double Y { get; set; } // Mouse Y position in the div
            public double MaxWidth { get; set; } // Original width of the div
            public double MaxHeight { get; set; } // Original height of the div
            public double DivRenderedWidthPx { get; set; } // Rendered width of the div
            public double DivRenderedHeightPx { get; set; } // Rendered height of the div
            public double ScreenWidthPx { get; set; } // Full screen width
            public double ScreenHeightPx { get; set; } // Full screen height
            public double RectWidth { get; set; } // Width of the bounding rectangle
            public double RectHeight { get; set; } // Height of the bounding rectangle
            public double RectLeft { get; set; } // Left position of the bounding rectangle
            public double RectTop { get; set; } // Top position of the bounding rectangle
            public double WindowX { get; set; } // Horizontal scroll offset of the window
            public double WindowY { get; set; } // Vertical scroll offset of the window
            public string Button { get; set; } // Mouse button type (e.g., "left", "right", "middle")
            public string EventType { get; set; } // Event type (e.g., "click", "mousemove")
        

    }
}
