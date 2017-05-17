<?

$total = $_GET[totalclients];

$fp = fopen('players.txt', 'w');
fwrite($fp, $total);


?>