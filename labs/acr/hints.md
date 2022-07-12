# Lab Hints

There are a few parts to this. Firstly image versions are grouped together into _repositories_ - you'll see separate repositories for your Nginx and simple-web images. There are AZ commands to work with repositories.

Each repository can have multiple images with different tags - `6.0` and `latest` are the tags for the simple-web image. You can list those tags with the CLI and sort them by creation time.

Then to delete an image you need to use the full name - registry domain plus repository plus tag.

> Need more? Here's the [solution](solution.md).