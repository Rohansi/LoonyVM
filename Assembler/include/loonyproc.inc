macro ccall proc,[arg]
{
common
	size@ccall = 0
	
	if ~ arg eq
reverse
		push arg
		size@ccall = size@ccall + 4
common
	end if
	
	call proc
	if size@ccall
		add sp, size@ccall
	end if
}

; -----------------------------------------------------------




