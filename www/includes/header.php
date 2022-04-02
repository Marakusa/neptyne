<?php
if (isset($_COOKIE['neptyne-theme'])) {
    $theme = $_COOKIE['neptyne-theme'];
}
else {
    $theme = 0;
}

$path = "";
if (isset($_GET['path'])) {
    $path = $_GET['path'];
    $pathBackings = "";
    $i = 0;
    $pathPartsCount = count(explode("/", $path));
    while($i < $pathPartsCount) {
        $pathBackings .= "../";
        $i++;
    }
}
?>
<head>
    <title>Neptyne</title>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link href="https://fonts.googleapis.com/css2?family=Fredoka:wght@300;400;500;600;700&display=swap" rel="stylesheet">
    <?php
    if ($theme != 0) {
        echo '<link rel="stylesheet" href="' . $pathBackings . 'css/style-dark.css">';
    }
    else {
        echo '<link rel="stylesheet" href="' . $pathBackings . 'css/style-light.css">';
    }
    ?>
    <link rel="stylesheet" href="<?php echo $pathBackings; ?>css/style.css">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
</head>

<header>
    <div style="cursor: pointer;" onclick="window.location.href = '<?php echo $pathBackings; ?>./';"><div class="logo"></div></div>
    <a href="<?php echo $pathBackings; ?>./#about">About</a>
    <a href="<?php echo $pathBackings; ?>download">Download</a>
    <a href="<?php echo $pathBackings; ?>documentation">Documentation</a>
    <a href="<?php echo $pathBackings; ?>news">News</a>
    <a href="https://github.com/Marakusa/neptyne">Source Code</a>
    <div style="width: 100%;padding-right: 30px;">
        <label class="switch" onClick="toggleDarkLight();" style="float: right;">
            <input id="dark-light-checkbox" type="checkbox" <?php
            if ($theme != 0) {
                echo "checked";
            }
            ?> >
            <span class="slider round"></span>
        </label>
    </div>

<script>
function toggleDarkLight() {
    var checkbox = document.getElementById('dark-light-checkbox');
    var head = document.getElementsByTagName('HEAD')[0];
    
    for (let i = 0; i < head.children.length; i++) {
        const element = head.children[i];
        if (element.rel === 'stylesheet') {
            if (element.href.endsWith('style-light.css') || element.href.endsWith('style-dark.css')) {
                element.href = checkbox.checked ? '<?php echo $pathBackings; ?>css/style-dark.css' : '<?php echo $pathBackings; ?>css/style-light.css';
                document.cookie = "neptyne-theme=" + (checkbox.checked ? "1" : "0") + "; path=/";
                break;
            }
        }
    }
}
</script>
</header>

<div class="page">