﻿using System;
using System.Collections.Generic;


public class Fabricator : TechnicalBlock
{
    int recipeID;
    Recipe recipe;

    int[] inputIDs;
    int outputID;

    int[] inputAmounts;
    int outputAmount;

    float sinceLastCraft;


    public Fabricator(byte lookDirection) : base(lookDirection)
    {
        inputIDs = new int[4];
        inputAmounts = new int[4];

        updatesNeighbours = true;

        this.lookDirection = lookDirection;
        requestedNeighbours = new byte[1];
        requestedNeighbours[0] = lookDirection;
    }
    public override void UpdateNeighbour(float deltaTime, TechnicalBlock[] neighbours)
    {
        if (neighbours.Length > 0)
        {
            TechnicalBlock target = neighbours[0];
            TryTransferingOutputToNeighbour(target);
        }
    }
    public void SwitchRecipe(int recipeID, ItemsContainer items)
    {
        this.recipeID = recipeID;
        recipe = items.recipes[recipeID];
        for(int i = 0; i < 4; i++)
        {
            inputIDs[i] = recipe.inputIDs[i];
            inputAmounts[i] = 0;
        }
        outputAmount = 0;
        outputID = recipe.outputID;
    }
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (recipeID != 0) // recipe is not null
        {
            sinceLastCraft += deltaTime; // craft progress
            TryFabricate();
        }
    }
    // no need for updating neighbours, conveyor does all the carrying around
    void TryTransferingOutputToNeighbour(TechnicalBlock target)
    {
        int output = CanOutput();
        if (output != 0)
        {
            if (target.CanTake(output))
            {
                Output();
                target.Take(output);
            }
        }
    }
    void Fabricate(float overflow)
    {
        sinceLastCraft = overflow;
        for (int i = 0; i < 4; i++)
        {
            inputAmounts[i] -= recipe.inputAmounts[i];
        }
        outputAmount += recipe.outputAmount;
    }
    void TryFabricate()
    {
        float overflow = 0;
        if(CooledDown(out overflow) && GotEnoughMaterials())
        {
            Fabricate(overflow);
        }
    }
    bool CooledDown(out float overflow)
    {
        if(sinceLastCraft > recipe.craftDuration)
        {
            overflow = sinceLastCraft - recipe.craftDuration;
            
            return true;
        }
        else
        {
            overflow = 0;
            return false;
        }
    }
    bool GotEnoughMaterials()
    {
        for(int i = 0; i< 4; i++)
        {
            if(inputAmounts[i] < recipe.inputAmounts[i]) { return false; }
        }
        return true;
    }

    public override bool CanTake(int itemID)
    {
        for(int i = 0; i < 4; i++)
        {
            if(inputIDs[i] == itemID && inputAmounts[i] < recipe.inputAmounts[i] * 2)
            {
                return true;
            }
        }
        return false;
    }
    public override void Take(int itemID)
    {
        for (int i = 0; i < 4; i++)
        {
            if (inputIDs[i] == itemID)
            {
                inputAmounts[i] += 1;
            }
        }
    }
    public override int CanOutput()
    {
        if(outputAmount != 0)
        {
            return outputID;
        }
        return 0;
    }
    public override int Output()
    {
        outputAmount -= 1;
        return outputID;
    }
}
