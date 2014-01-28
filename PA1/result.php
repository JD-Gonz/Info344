<?php
$name = $_POST["name"];
$username = "info344user";
$password = "Godspeed23";

// Connects to Database and reform query
try {
	$conn = new PDO('mysql:host=info344.cf9rvll9cbvo.us-west-2.rds.amazonaws.com:3306;dbname=mydb', $username, $password);
	$stmt = $conn->prepare("SELECT * FROM PlayerStats WHERE PlayerName like '%$name%'");
	$stmt -> execute(array());
} catch(PDOException $e) {
	echo 'ERROR: ' . $e->getMessage();
}
?>
<html>
	<head>
		<link rel="stylesheet" href="https://netdna.bootstrapcdn.com/bootstrap/3.0.0/css/bootstrap.min.css">
		<link rel="stylesheet" href="css/styles.css">
		<title>Your Results </title>
	</head>
	<body>
		<div class="container">
			<h1>Your Results</h1>
			<h3>Player's Name | GP | FGP | TPP | FTP | PPG |</h3>
			<?php 
			//get resutls and display them
			$result = $stmt -> fetchAll(PDO::FETCH_ASSOC);
			if ( count($result) ) {
				foreach($result as $row) {
					foreach($row as $key => $value) {
						echo $value . " | ";
					}
					echo "<br><br>";
				}
			} else {
				echo "No Results Found";
			}
			?>
		</div>
		<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.10.2/jquery.min.js"></script>
		<script src="https://netdna.bootstrapcdn.com/bootstrap/3.0.0/js/bootstrap.min.js"></script>
	</body>
</html>





