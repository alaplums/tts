# Guide to contributing

Please read this if you intend to contribute to the project.

In order for any contributions to be accepted you MUST do the following things.

Every commit you make in your patch or pull request MUST be "signed off".

You do this by adding the `-s` flag when you make the commit(s), e.g.

    git commit -s -m "Shave the yak some more"

## Making your changes

### Setting up a ALAP development environment

We are using Eclipse Neon as development IDE and using the 4.6 release as ALAP target platform.

#### Manual setup

Use Eclipse to check out the repo.

1. Install [Eclipse IDE for Committers](http://www.eclipse.org/downloads/). Other versions may work. These instructions were tested using Neon under Mac OS X and Windows 10.
2. Start Eclipse. Create a new workspace.  You may need to close the Welcome tab by clicking on the X.
3. Open **Window > Show View > Other > Git > Git Repositories**
4. Click on **Clone a Git repository**
5. Click on **Clone URI**, then **Next**
6. Enter the URI ``https://github.com/alaplums/tts/``
7. In the Branch Selection window, keep the default of the Master branch and click Next.
8. In the Local Destination window, select **Finish**.

### Create an Issue
Create a [GitHub Issue](https://github.com/alaplums/tts/issues) for every significant piece of work ( > 2 hrs).

### Create a new branch for your changes

1. In the Git Repositories tab, expand the *tts* repository.
2. Right click on the "Branches" node and select "Switch To" -> "New Branch".  
3. Enter the new branch name.  
Branch name should be {GitHubUserName}/{summary or issue id} e.g. ``erwin/integrate-display-actor``.  

### Committing
* Make your changes.
* Commit your changes into that branch. 
* For files that are in Eclipse packages, right click on the file in the Package Explorer and commit it.  
* For files that are not in Eclipse packages, invoke the Git Staging via Window -> Show View -> Other -> Git -> Git
* Use descriptive and meaningful commit messages. See [git commit records in the Eclipse Project Handbook](https://www.eclipse.org/projects/handbook/#resources-source).  Mention issue_id in each commit comment using syntax like "Adapt this interface for #15" to link to issue 15.
* Make sure you use the sign off your commit.
  * If you are commiting using Eclipse, then click on the signature button  
  * If you are invoking git from the command line, then use the `-s` flag.  
  * If you are using some other tool, add ``Signed-off-by: YourFirstName YourLastName`` For example: ``Signed-off-by: Christopher Brooks``
* Push your changes to your branch in your forked repository.

## Submitting the changes

1. Submit a pull request via the normal [GitHub UI](https://github.com/alaplums/tts/issues) to trigger to request feedback / code review / ... 
2. Mention issue_id in each comment using syntax like "Adapt this interface for #15" to link to issue 15 in the initial comment for a Pull Request.
3. The pull request will be reviewed by one of the committers, and then merged into master.
 
## After submitting

* Do not use your branch for any other development, otherwise further changes that you make will be visible in the PR.

# Credit

This file is based on a file written by the Vert.x team at https://raw.githubusercontent.com/eclipse/vert.x/master/CONTRIBUTING.md

We have copied, modified and co-opted it for our own repo and we graciously acknowledge the work of the original authors.
