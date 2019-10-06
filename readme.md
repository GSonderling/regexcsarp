#REGEXCSARP

Let's say you want to reimplement grep, or a grep like tool. 
Is it a good idea? Is it going to be useful? Are you **going to use it**, after you actually finish the job?
**Will you finish the job?**

Answer to most of these questions is probably: *Nah*.

But it is going to be an interesting work nonetheless. I hope...

##Goals and objectives

It does look straightforward enough, but we should give it some structure.
###The ~~Impossibly~~ Idealistic foundations
1. Avoid more sophisticated .NET classes and features. 
	* No linq
	* No specialized collections
	* No regex (Duh!)
2. Keep things within bounds of formally defined regular grammars. 
3. Stick to conventions of POSIX BRE, if possible..

###The ~~Required~~ Features and Functions

1. Alternation, Kleene star, concatenation...
2. Basic character classes.
3. Implement ^ and $.
4. Advanced, meaning arbitrary, quantification.


