<?php

$name = $_GET["name"];
$username = "info344user";
$password = "Godspeed23";

// Connects to Database and reform query
try {
	$conn = new PDO('mysql:host=mydbinstance.cf9rvll9cbvo.us-west-2.rds.amazonaws.com:3306;dbname=mydb', $username, $password);
	$stmt = $conn->prepare("SELECT * FROM PlayerStats WHERE PlayerName = '$name'");
	$stmt -> execute(array());
} catch(PDOException $e) {
	echo 'ERROR: ' . $e->getMessage();
}
//get resutls and display them
$result = $stmt -> fetchAll(PDO::FETCH_ASSOC);
echo $_GET['callback'] . '(' . json_encode($result) . ')';

?>






