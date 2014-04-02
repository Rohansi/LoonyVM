
macro invoke proc,[arg]
{
common
    if ~ arg eq
reverse
        push arg
common
    end if
    call proc
}

macro invoke_va proc,[arg]
{
common
    @cnt = 0
    if ~ arg eq
reverse
        push arg
        @cnt = @cnt + 1
common
    end if
    push @cnt
    call proc
    if @cnt
        add sp, (@cnt * 4)
    end if
}
