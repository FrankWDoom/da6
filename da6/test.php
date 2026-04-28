<?php
error_reporting(E_ALL & ~E_NOTICE & ~E_WARNING);

echo "test\n";

$a =  array( 0, 'BRK', 1, 0, 0);
$l = count($a);
//echo "a: len: $l val: $a \n";

foreach ($a as $n => $v) {
	//echo "n: $n v: $v \n";
}
 
$a[] = "xyz";
//echo "a: $a \n";

$l = count($a);
//echo "a: len: $l val: $a \n";

foreach ($a as $n => $v) {
	//echo "n: $n v: $v \n";
}

$hstr = "f00f";

echo("str: $hstr \n");
echo("dec: " . hexdec($hstr) . "\n");

$hstr = null;
echo("str: null \n");
echo("dec: " . hexdec($hstr) . "\n");

$hstr = "";
echo("str: empty \n");
echo("dec: " . hexdec($hstr) . "\n");

$hstr = "blerg";
echo("str: $hstr \n");
echo("dec: " . hexdec($hstr) . "\n");

echo(FILE_APPEND . PHP_EOL); 

var_dump($argv);
