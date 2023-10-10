;******************************************************
; 
; Project: "Obliczanie odchylenia standardowego próby"
; Author: Szczepan Dwornicki
; Date: 5th term, 2022 / 2023
;
;******************************************************

.data
n dd ? ; number of elements
n_init dd ? ; n initial value (use to restore n value)
element dd ? ; represents single element of sample
shift dq ? ; to restore array adress
zero dd 0 ; to clear the xmm2
result dd ? ; value of standard deviation
i dd ? ; loop variable

.code
standardDeviationAsm proc
 ; Initial number of elements 
 MOV n_init, edx

 ; Number of elements. Variable later used in loops
 mov n, edx

 ; Calculate mean - Add elements of sample

 ; Place the first element in xmm0
 mov eax, [rcx]
 mov element, eax
 MOVSS xmm0, element
 sub n, 1
 
adding:
 cmp n, 0
 jle adding_done
 sub n, 1
 ; Point to the next element
 add rcx, 4
 mov eax, [rcx]
 mov element, eax
 ADDSS xmm0, element
 JMP adding
adding_done:

 ; Restore n value
 MOV eax, n_init
 mov n, eax
 ; Restore address of the first element in array
 sub n, 1
 MOV eax, 4
 MUL n
 MOV shift, rax
 sub rcx, shift
 add n, 1

 ; Calculate the mean - divide sum of elements by number of them
 cvtsi2ss xmm1, n
 DIVSS xmm0, xmm1

 ; Place the mean in ymm1
 VBROADCASTSS ymm1, xmm0

 ; Calculate the difference between mean and each element and calculate its square. Add the results

 ; Clear the xmm2, which will hold the sum of squares of differences
 cvtsi2ss xmm2, zero

 ; If n >= 8, use vector instructions, if not, use scalar ones
subtracting_and_square_vector:

 VMOVUPS ymm0, [rcx]

 ; Subtract mean from each element
 VSUBPS ymm0, ymm0, ymm1

 ; Calculate the square of each difference
 VMULPS ymm0, ymm0, ymm0

 ; Add squares and place the result in xmm2

 VMOVUPS [rcx], ymm0

 ; Initialize loop variable
 ; Assume that there are at least 8 elements in array
 MOV i, 8
 cmp n, 8
 JGE adding_squares
 ; Place actual number of elements in array
 mov eax, n
 MOV i, eax

 adding_squares:
 cmp i, 0
 jle adding_squares_done
 sub i, 1
 
 mov eax, [rcx]
 mov element, eax
 ADDSS xmm2, element
 ADD rcx, 4

 JMP adding_squares

 adding_squares_done:

 sub n, 8
 cmp n, 1
 JGE subtracting_and_square_vector

 ; Divide the sum by n - number of elements
 cvtsi2ss xmm0, n_init
 DIVSS xmm2, xmm0

 ;Calculate the square root
 SQRTSS xmm2, xmm2
 MOVSS xmm0, xmm2

done:
ret
standardDeviationAsm endp
end







