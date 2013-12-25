
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
    @count = 0
    if ~ arg eq
reverse
    push arg
    @count = @count + 1
common
    end if
    push @count
    call proc
    if @count
        add sp, (@count * 4)
    end if
}
