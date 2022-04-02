<?php
include 'includes/header.php';

$host = "https://raw.githubusercontent.com/Marakusa/neptyne/master/docs/";
$editHost = "https://github.com/Marakusa/neptyne/blob/master/docs/";

if ($pathPartsCount === 1 && $path === "guide") {
    $pathBackings = "";
}
require $pathBackings . 'vendor/autoload.php';
use Michelf\Markdown;
?>

<div class="documentation-page">
<div class="documentation-sidebar">
    <a class="sidebar-doc-link" href="<?php echo $pathBackings; ?>documentation">Neptyne documentation</a>
    <a class="sidebar-doc-link" href="<?php echo $pathBackings; ?>documentation/Compiler%20Errors/Overview">Copmiler Errors</a>
    <a class="sidebar-doc-link" href="<?php echo $pathBackings; ?>documentation/Examples/Overview">Examples</a>
    <a class="sidebar-doc-link" href="<?php echo $pathBackings; ?>documentation/Keywords/Overview">Keywords</a>
</div>
<div class="page-content">
<?php

if ($path != "") {
    $path = str_replace(" ", "%20", $path);
    $urlPath = $host . $path . ".md";
}
else {
    $urlPath = $host . "Main.md";
}
$contents = file_get_contents($urlPath);

if ($contents == "") {
    $contents = file_get_contents("notfound.md");
}

$contents = str_replace("```npt\n", "<pre><code>", $contents);
$contents = str_replace("\n```", "</code></pre>", $contents);
$md = Markdown::defaultTransform($contents);
$md = str_replace(":::row:::", "<section class=\"row\">", $md);
$md = str_replace(":::row-end:::", "</section>", $md);
$md = str_replace(":::column:::", "<section class=\"column\">", $md);
$md = str_replace(":::column-end:::", "</section>", $md);
echo "<div class=\"docs-edit-tools\"><a href=\"" . $editHost . $path . ".md\">🖉 Edit</a></div>";
echo "<div class=\"content\">" . $md . "</div>";

?>
</div>
</div>