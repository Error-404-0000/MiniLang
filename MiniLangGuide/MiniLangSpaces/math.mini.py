fn number add(number1, number2) {
	give number1 + number2;
}
fn number subtract(number1, number2) {
	give number1 - number2;
}
fn number multiply(number1, number2) {
	give number1 * number2;
}

fn number divide(number1,number2){
         if(number2 ==0):
	 {
		 print("Error: Division by zero is not allowed.");
		 give 0;
        }
	 else
		 give number1 / number2;
	 done
}
fn number modulo(number1,number2){
	 if(number2 ==0):
	 {
	     print("Error: Division by zero is not allowed.");
	     give 0;
	 }
	 else
		 give number1 % number2;
	 done
}
fn number power(number1,number2){
	 if(number2 ==0):
		 give 1;
	 else
		 give number1 ^ number2;
	 done
}
fn number squareRoot(number1) {
	if (number1 < 0):
		print("Error: Cannot calculate square root of a negative number.");
		give 0;
	else
		give number1 ^ 0.5;
	done
}
