<?php
$name = $_POST["name"];
$username = "info344user";
$password = "Godspeed23";

// Connects to your Database
try {
	$conn = new PDO('mysql:host=info344.cf9rvll9cbvo.us-west-2.rds.amazonaws.com:3306;dbname=innodb', $username, $password);
	$stmt = $conn->prepare("SELECT * FROM PlayerStats WHERE PlayerName like '%$name%'");
	$stmt -> execute(array());
	
	$result = $stmt -> fetchAll(PDO::FETCH_ASSOC);
	
	if ( count($result) ) {
		foreach($result as $row) {
			 print_r($row);
			 print("\n");
		}
	} else {
		echo "No rows returned.";
	}
} catch(PDOException $e) {
	echo 'ERROR: ' . $e->getMessage();
}
?>
<html>
<head>
<title>Results for $name</title>
</head>
<body>
<?php 

?>
</body>
</html>


<!--  Perform database query -->
<!--  $query = "SELECT * FROM PlayerStats WHERE PlayerName like '%$name%'"; -->
<!--  $results = mysql_query($query) or die('Query failed: ' . mysql_error());; -->

<!--  while ($row = mysql_fetch_assoc($results)) { -->
<!--      echo $row["PlayerName"] . " "; -->
<!--      echo $row["GP"] . " "; -->
<!--      echo $row["FGP"] . " "; -->
<!--      echo $row["TPP"] . " "; -->
<!--      echo $row["FTP"] . " "; -->
<!--      echo $row["PPG"] . "<br>"; -->
    
<!--  } -->


