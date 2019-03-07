Public Module Number
    Public Function RoundUp(ByVal nNumber As Object, Optional ByVal nDecimalPlaces As Integer = 2, Optional ByVal nSplitNo As Integer = 5) As Decimal
        Try
            'get the dross over with
            If Not IsNumeric(nNumber) Then Return 0
            'no decimal places to deal with
            If Not nNumber.ToString.Contains(".") Then Return nNumber
            'has correct number of decimal places
            If Split(nNumber.ToString, ".")(1).Length <= nDecimalPlaces Then Return nNumber

            'now the fun
            Dim nWholeNo As Integer = Split(nNumber.ToString, ".")(0) 'the whole number before decimal point
            Dim nTotalLength As Integer = Split(nNumber.ToString, ".")(1).Length 'the total number of decimal places

            Dim nI As Integer 'a counter

            Dim nCarry As Integer = 0 'number to carry to next number

            'loop through until we reach the correct number of decimal places
            For nI = 0 To nTotalLength - nDecimalPlaces
                Dim nCurrent As Integer 'the number we are working on
                nCurrent = Right(Left(Split(nNumber.ToString, ".")(1), nTotalLength - nI), 1)
                nCurrent += nCarry 'add the carry
                If nCurrent >= nSplitNo Then nCarry = 1 Else nCarry = 0 'make a new carry dependant on whaere we are
            Next
            Dim nDecimal As Integer = Left(Split(nNumber.ToString, ".")(1), nDecimalPlaces) 'the decimal value
            nDecimal += nCarry 'add last carry
            If nDecimal.ToString.Length > nDecimalPlaces Then 'if we have now gone over the number of decimal places then need to sort it
                nCarry = 1
                nDecimal = Right(nDecimal.ToString, nDecimalPlaces)
            Else
                nCarry = 0
            End If
            nWholeNo += nCarry
            Return CDec(nWholeNo & "." & nDecimal)
        Catch ex As Exception
            Return 0
        End Try
    End Function

    Public Function MinimumValue(ByVal Value As Double, ByVal Min As Double) As Double
        Try
            If Value < Min Then Return Min Else Return Value
        Catch ex As Exception
            Return Min
        End Try
    End Function

    Public Function ConvertStringToIntegerWithFallback(ByVal input As String, Optional ByVal defaultInteger As Integer = 0) As Integer
        Try
            If IsReallyNumeric(input) Then
                Return Convert.ToInt32(input)
            Else
                Return defaultInteger
            End If

        Catch ex As Exception
            Return defaultInteger
        End Try
    End Function

    Public Function CheckAndReturnStringAsNumber(ByVal input As String, ByRef numberReturn As Object, ByVal numericType As Type) As Boolean
        Dim isNumber As Boolean = False
        Try

            If IsReallyNumeric(input) Then
                ' Try to Convert this to the right type
                numberReturn = Convert.ChangeType(input, numericType)
                isNumber = True

            End If

        Catch ex As Exception


        End Try

        Return isNumber

    End Function
    Public Function IsEven(ByVal inputNumber As Long) As Boolean
        Return (inputNumber Mod 2 = 0)
    End Function

    Public Function IsOdd(ByVal inputNumber As Long) As Boolean
        Return Not IsEven(inputNumber)
    End Function

    Public Function IsStringNumeric(ByVal input As String) As Boolean
        Return Not (String.IsNullOrEmpty(input)) AndAlso IsNumeric(input)
    End Function


    ''' <summary>
    ''' IsNumeric is rubbish (http://classicasp.aspfaq.com/general/what-is-wrong-with-isnumeric.html)
    ''' Here's a string check. 
    ''' The regular expression can be summed up this way
    ''' 1. "-" or "+" character, zero or one time
    ''' 2. one or more digit
    ''' 3. Optionally: "." followed by one or more digit
    ''' </summary>
    ''' <param name="input"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsReallyNumeric(ByVal input As String) As Boolean
        Try
            Return System.Text.RegularExpressions.Regex.IsMatch(input, "^(-|\+)?\d+(\.\d+)?$")
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Random - replaces the awful System.Random with the Mersenne Twister
    ''' </summary>
    ''' <remarks>Taken from http://ilovevb.net/Web/blogs/heroicadventure/archive/2008/02/19/a-more-random-random.aspx, based on http://www.math.sci.hiroshima-u.ac.jp/~m-mat/MT/emt.html).</remarks>
    Public Class Random
        Private Const N As Integer = 624
        Private Const M As Integer = 397
        Private Const MATRIX_A As UInteger = &H9908B0DFUI
        Private Const UPPER_MASK As UInteger = &H80000000UI
        Private Const LOWER_MASK As UInteger = &H7FFFFFFFUI

        Private mt(N - 1) As UInteger
        Private mti As Integer = N + 1
        ''' <summary>
        ''' Create a new Mersenne Twister random number generator.
        ''' </summary>
        Public Sub New()
            Me.New(CUInt(Date.Now.Millisecond))
        End Sub

        ''' <summary>
        ''' Create a new Mersenne Twister random number generator with a
        ''' particular seed.
        ''' </summary>
        ''' <param name="seed">The seed for the generator.</param>
        <CLSCompliant(False)> _
        Public Sub New(ByVal seed As UInteger)
            mt(0) = seed
            For mti = 1 To N - 1
                mt(mti) = CUInt((1812433253UL * (mt(mti - 1) Xor (mt(mti - 1) >> 30)) + CUInt(mti)) And &HFFFFFFFFUL)
            Next
        End Sub

        ''' <summary>
        ''' Create a new Mersenne Twister random number generator with a
        ''' particular initial key.
        ''' </summary>
        ''' <param name="initialKey">The initial key.</param>
        <CLSCompliant(False)> _
        Public Sub New(ByVal initialKey() As UInteger)
            Me.New(19650218UI)
            Dim i, j, k As Integer
            i = 1 : j = 0
            k = CInt(IIf(N > initialKey.Length, N, initialKey.Length))
            For k = k To 1 Step -1
                mt(i) = CUInt(((mt(i) Xor ((mt(i - 1) Xor (mt(i - 1) >> 30)) * 1664525UL)) + initialKey(j) + CUInt(j)) And &HFFFFFFFFUI)
                i += 1 : j += 1
                If i >= N Then mt(0) = mt(N - 1) : i = 1
                If j >= initialKey.Length Then j = 0
            Next
            For k = N - 1 To 1 Step -1
                mt(i) = CUInt(((mt(i) Xor ((mt(i - 1) Xor (mt(i - 1) >> 30)) * 1566083941UL)) - CUInt(i)) And &HFFFFFFFFUI)
                i += 1
                If i >= N Then mt(0) = mt(N - 1) : i = 1
            Next
            mt(0) = &H80000000UI
        End Sub

        ''' <summary>
        ''' Generates a random number between 0 and System.UInt32.MaxValue.
        ''' </summary>
        <CLSCompliant(False)> _
        Public Function NextUInt32() As UInteger

            Dim y As UInteger
            Static mag01() As UInteger = {&H0UI, MATRIX_A}
            If mti >= N Then
                Dim kk As Integer
                Debug.Assert(mti <> N + 1, "Failed initialization")
                For kk = 0 To N - M - 1
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + M) Xor (y >> 1) Xor mag01(CInt(y And &H1))
                Next
                For kk = kk To N - 2
                    y = (mt(kk) And UPPER_MASK) Or (mt(kk + 1) And LOWER_MASK)
                    mt(kk) = mt(kk + (M - N)) Xor (y >> 1) Xor mag01(CInt(y And &H1))
                Next
                y = (mt(N - 1) And UPPER_MASK) Or (mt(0) And LOWER_MASK)
                mt(N - 1) = mt(M - 1) Xor (y >> 1) Xor mag01(CInt(y And &H1))
                mti = 0
            End If
            y = mt(mti)
            mti += 1
            ' Tempering
            y = y Xor (y >> 11)
            y = y Xor ((y << 7) And &H9D2C5680UI)
            y = y Xor ((y << 15) And &HEFC60000UI)
            y = y Xor (y >> 18)
            Return y
        End Function

        ''' <summary>
        ''' Generates a random integer between 0 and System.Int32.MaxValue.
        ''' </summary>
        Public Function [Next]() As Integer
            Return CInt(NextUInt32() >> 1)
        End Function

        ''' <summary>
        ''' Generates a random integer between 0 and maxValue.
        ''' </summary>
        ''' <param name="maxValue">The maximum value. Must be greater than zero.</param>
        Public Function [Next](ByVal maxValue As Integer) As Integer
            Return [Next](0, maxValue)
        End Function

        ''' <summary>
        ''' Generates a random integer between minValue and maxValue.
        ''' </summary> 
        ''' <param name="maxValue">The lower bound.</param>
        ''' <param name="minValue">The upper bound.</param>
        Public Function [Next](ByVal minValue As Integer, ByVal maxValue As Integer) As Integer
            Return CInt(Math.Floor((maxValue - minValue + 1) * NextDouble() + minValue))
        End Function

        ''' <summary>
        ''' Generates a random floating point number between 0 and 1.
        ''' </summary>
        Public Function NextDouble() As Double
            Return NextUInt32() * (1.0 / 4294967295.0)
        End Function
    End Class

End Module


