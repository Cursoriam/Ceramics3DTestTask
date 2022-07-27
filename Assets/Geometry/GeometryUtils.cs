using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeometryUtils : MonoBehaviour
{
    public static bool IsSegmentIntersecting(Vector2 firstLineStart, Vector2 firstLineEnd, Vector2 secondLineStart,
        Vector2 secondLineEnd)
    {
        var isIntersecting = false;

        var firstLineDirection = (firstLineEnd - firstLineStart).normalized;
        var secondLineDirection = (secondLineEnd - secondLineStart).normalized;

        var firstLineNormal = new Vector2(-firstLineDirection.y, firstLineDirection.x);
        var secondLineNormal = new Vector2(-secondLineDirection.y, secondLineDirection.x);

        var a = firstLineNormal.x;
        var b = firstLineNormal.y;

        var c = secondLineNormal.x;
        var d = secondLineNormal.y;
        
        var k1 = (a * firstLineStart.x) + (b * firstLineStart.y);
        var k2 = (c * secondLineStart.x) + (d * secondLineStart.y);

        if (IsParallel(firstLineNormal, secondLineNormal))
            return isIntersecting;

        if (IsOrthogonal(firstLineStart - secondLineStart, firstLineNormal))
        {
            isIntersecting = true;
            return isIntersecting;
        }

        var intersectionPointX = (d * k1 - b * k2) / (a * d - b * c);
        var intersectionPointY = (-c * k1 + a * k2) / (a * d - b * c);

        var intersectionPoint = new Vector2(intersectionPointX, intersectionPointY);

        if (IsBetween(firstLineStart, firstLineEnd, intersectionPoint) &&
            IsBetween(secondLineStart, secondLineEnd, intersectionPoint))
            isIntersecting = true;

        return isIntersecting;
    }

    private static bool IsParallel(Vector2 vector1, Vector2 vector2)
    {
        if (Vector2.Angle(vector1, vector2) == 0f || Math.Abs(Vector2.Angle(vector1, vector2) - 180f) < 0.000001f)
            return true;

        return false;
    }

    private static bool IsOrthogonal(Vector2 vector1, Vector2 vector2)
    {
        if (Mathf.Abs(Vector2.Dot(vector1, vector2)) < 0.000001f)
            return true;

        return false;
    }

    private static bool IsBetween(Vector2 a, Vector2 b, Vector2 c)
    {
        bool isBetween = false;

        var ab = b - a;
        var ac = c - a;

        if (Vector2.Dot(ab, ac) > 0f && ab.sqrMagnitude >= ac.sqrMagnitude)
            isBetween = true;

        return isBetween;
    }
}
