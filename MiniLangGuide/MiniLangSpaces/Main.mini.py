use "console.mini.py";
use "math.mini.py";

@
@
@
@                      USING THE .PY EXETENTION FOR COLOR HIGHLIGHT
@
@


fn object do(){
  make i  = 200;
  return(200);
}
print("This is a test of the MiniLangSpaces console.");
do();

make eee = 2;
while(eee!=2000*233+2000){
    eee+=1;
    print(eee);
}
@@ --- Variables and Operations ---
make  a = 10;
make  b = 5;
make  c = add(a, b);

print("a = ", a, ", b = ", b, ", c = ", c);

@@ --- if/else and Conditions ---
if (a > b):
    print("a is greater than b");
else
    print("a is not greater than b");
done

@@ --- Function creation and give ---
fn number max(num1, num2) {
    if (num1 > num2):
        give num1;
    else
        give num2;
    done
}

print("Max of a and b: ", max(a, b));

@@--- Scope demonstration ---
{
    make scopedVar = 42;
    print("Inside scope, scopedVar = ", scopedVar);
}





@@ --- while loop ---


make  sum = 0;
make  i = 1;

while (i <= 5):
    sum = add(sum, i);
    print("i = ", i, ", sum = ", sum);
    i = add(i, 1);
done

print("Final sum from 1 to 5: ", sum);

print("This is a say/print demonstration.");

print("Subtract: ", subtract(10, 4));
print("Multiply: ", multiply(6, 7));
print("Divide: ", divide(20, 5));
print("Modulo: ", modulo(10, 3));
print("Power: ", power(2, 8));
print("Square Root: ", squareRoot(16));




print("Add: ", add(2, 3));
print("Subtract: ", subtract(10, 4));
print("Multiply: ", multiply(6, 7));
print("Divide: ", divide(20, 5));
print("Divide by zero: ", divide(10, 0));
print("Modulo: ", modulo(10, 3));
print("Modulo by zero: ", modulo(10, 0));
print("Power: ", power(2, 8));
print("Power with zero exponent: ", power(5, 0));
print("Square Root: ", squareRoot(16));
print("Square Root of negative: ", squareRoot(9));

{
    make  sum = 0; 
    make  i = 1;
    while (i <= 5) @the while loop takes an expression which means anything that returns a number is still valid while(2*2){} etc
    {
        sum = add(sum, i);
        print("i = ", i, ", sum = ", sum);
        i += add(i, i);
    }
    print("Final sum from 1 to 5: ", i , "and the sum is $(sum)");
}    



